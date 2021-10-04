/* Matthew Delapasse
 * September 13 2021
 * My program is a build off of example 5-8 in the book
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace AuthorsTableInputForm6_7
{
    public partial class frmAuthors : Form
    {
        public frmAuthors()
        {
            InitializeComponent();
        }

        //level declarations that will be used in the frmAuthors_Load
        SqlConnection booksConnection;
        SqlCommand authorsCommand;
        SqlDataAdapter authorsAdapter;
        DataTable authorsTable;
        CurrencyManager authorsManager;
        string myState;
        int myBookmark;


        private void frmAuthors_Load(object sender, EventArgs e)
        {
            try
            {
                //point to help file
                hlpAuthors.HelpNamespace = Application.StartupPath + "\\authors.chm";

                //connect to the books database (this will lead to successful connection)
                //string fullfile = Path.GetFullPath("SQLBooksDB.mdf");

                //Connect to the books database (this will lead to an unsuccessful connection)
                //string fullfile = Path.GetFullPath("SQLBooksDB.accdb");

                booksConnection = new SqlConnection("Data Source=.\\SQLEXPRESS; AttachDbFilename=" + Application.StartupPath + "SQLBooks.DB.mdf;Integrated Security=True; Connect Timeout=30; User Instance=True");
                booksConnection.Open();

                //This tested to see if the connection worked
                //MessageBox.Show("the connection was successfull");

                //establish command object
                authorsCommand = new SqlCommand("SELECT * FROM Authors ORDER BY Author", booksConnection);

                ////connection object established
                //MessageBox.Show("The connection object established.");

                //esablish data adapter/data table
                authorsAdapter = new SqlDataAdapter();
                authorsAdapter.SelectCommand = authorsCommand;
                authorsTable = new DataTable();
                authorsAdapter.Fill(authorsTable);

                //bind controls to data table
                txtAuthorID.DataBindings.Add("Text", authorsTable, "Au_ID");
                txtAuthorName.DataBindings.Add("Text", authorsTable, "Author");
                txtYearBorn.DataBindings.Add("Text", authorsTable, "Year_Born");

                //establish currency manager
                authorsManager = (CurrencyManager)this.BindingContext[authorsTable];
                SetText();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error establishing Authors table.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            //When the applicaiton starts it will be in view state
            this.Show();
            SetState("View");
        }

        private void frmAuthors_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before stopping the application.", " ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                try
                {
                    //save changes to database
                    SqlCommandBuilder authorsAdapterCommands = new SqlCommandBuilder(authorsAdapter);
                    authorsAdapter.Update(authorsTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error saving database to file: \r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // close the connection 
                booksConnection.Close();

                //dispose of the objects
                booksConnection.Dispose();
                authorsCommand.Dispose();
                authorsAdapter.Dispose();
                authorsTable.Dispose();
            }
        }
        
        private void btnFirst_Click(object sender, EventArgs e)
        {
            authorsManager.Position = 0;
            SetText();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            authorsManager.Position = authorsManager.Count - 1;
            SetText();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try
            {
                myBookmark = authorsManager.Position;
                authorsManager.AddNew();
                SetState("Add");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (authorsManager.Position == 0)
            {
                Console.Beep();
            }
            authorsManager.Position--;
            SetText();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (authorsManager.Position == authorsManager.Count - 1)
            {
                Console.Beep();
            }
            authorsManager.Position++;
            SetText();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData())
            {
                return;
            }

            string savedName = txtAuthorName.Text;
            int savedRow;

            try
            {
                authorsManager.EndCurrentEdit();
                authorsTable.DefaultView.Sort = "Author";
                savedRow = authorsTable.DefaultView.Find(savedName);
                authorsManager.Position = savedRow;
                MessageBox.Show("Record saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetState("View");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            authorsManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                authorsManager.Position = myBookmark;
            }
            SetState("View");
            SetText();
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (response == DialogResult.No)
            {
                return;
            }
            try
            {
                authorsManager.RemoveAt(authorsManager.Position);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, hlpAuthors.HelpNamespace);
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtFind.Text.Equals(""))
            {
                return;
            }
            int savedRows = authorsManager.Position;
            DataRow[] foundRows;
            authorsTable.DefaultView.Sort = "Author";
            foundRows = authorsTable.Select("Author LIKE '" + txtFind.Text + "%'");
            if (foundRows.Length == 0)
            {
                authorsManager.Position = savedRows;
            }
            else
            {
                authorsManager.Position = authorsTable.DefaultView.Find(foundRows[0]["Author"]);
            }
            SetText();
        }

        private void txtYearBorn_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (int)e.KeyChar == 8)
            {
                //Acceptable keystrokes
                e.Handled = false;
            }
            else if ((int)e.KeyChar == 13)
            {
                //This sets its attention to the txtAuthorName
                txtAuthorName.Focus();
            }
            else
            {
                e.Handled = true;
                Console.Beep();
            }
        }
        private void txtAuthorName_KeyPress(Object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                txtYearBorn.Focus();
            }
        }

        private void SetState(string appState)
        {
            myState = appState;
            switch (appState)
            {
                case "View":
                    txtAuthorID.BackColor = Color.White;
                    txtAuthorID.ForeColor = Color.Black;
                    txtAuthorName.ReadOnly = true;
                    txtYearBorn.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnAddNew.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnEdit.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    grpFindAuthor.Enabled = true;
                    txtAuthorName.Focus();
                    break;
                default: // Add or Edit if not View;
                    txtAuthorID.BackColor = Color.Red;
                    txtAuthorID.ForeColor = Color.White;
                    txtAuthorName.ReadOnly = false;
                    txtYearBorn.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    grpFindAuthor.Enabled = false;
                    txtAuthorName.Focus();
                    break;
            }
        }

        private bool ValidateData()
        {
            string message = "";
            int inputYear, currentYear;
            bool allOK = true;

            // Check for name
            if (txtAuthorName.Text.Trim().Equals(""))
            {
                message = "You must enter an Author Name." + "\r\n";
                txtAuthorName.Focus();
                allOK = false;
            }

            //Check length and range on Year Born
            if (!txtYearBorn.Text.Trim().Equals(""))
            {
                inputYear = Convert.ToInt32(txtYearBorn.Text);
                currentYear = DateTime.Now.Year;
                if (inputYear > currentYear || inputYear < currentYear - 150)
                {
                    message += "Year born must be between " + (currentYear - 150).ToString() + " and " + currentYear.ToString();
                    txtYearBorn.Focus();
                    allOK = false;
                }
            }

            if (!allOK)
            {
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return (allOK);
        }

        private void SetText()
        {
            this.Text = "Authors - Record " + (authorsManager.Position + 1).ToString() + " of " + authorsManager.Count.ToString() + " Records";
        }
    }
}
