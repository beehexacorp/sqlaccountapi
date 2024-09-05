using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace AccountinglinkC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Int32 lBuildNo;
        dynamic ComServer;
        Type lBizType;

        public void KillApp()
        {
            try
            {
                foreach (Process prc in Process.GetProcessesByName("SQLAcc"))
                {
                    prc.Kill(); //Make sure no other SQLAcc is running
                }
                Thread.Sleep(2000); //Sleep for 2 seconds.
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        public void CheckLogin(int AType = 0, int ATDCF = 0)
        {            
            string ADB, ADCF;

            if (edLogout.Checked)
            {
               // KillApp();
            }

            lBizType = Type.GetTypeFromProgID("SQLAcc.BizApp");
            ComServer = Activator.CreateInstance(lBizType);
            
            if (AType == 0)
            {
                ADB = edDB.Text;
            } else
            {
                ADB = edDB2.Text;
            }

            if (ATDCF == 0)
            {
                ADCF = edDCF.Text;
            }
            else
            {
                ADCF = edDCF2.Text;
            }

            if (!ComServer.IsLogin)
            {
                try
                {
                    ComServer.Login(edUN.Text, edPW.Text, ADCF, ADB);
                    ComServer.Minimize();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    FreeBiz(ComServer);
                }
            }
            if (ComServer.IsLogin)
            {
                lBuildNo = ComServer.BuildNo;
            }
         }

        public void Logout()
        {
            if (edLogout.Checked)
            {
                ComServer.Logout();
                FreeBiz(ComServer);
            }
        }

        public void FreeBiz(object AbizObj)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(AbizObj);
        }

        public bool IsOdd(long lngNumber)
        {
            return !(lngNumber%2 == 0);
        }

        public string QuotedStr(string str)
        {
            return "'" + str.Replace("'", "''") + "'";
        }

        private static string FormatAsRTF(string DirtyText)
        {
            System.Windows.Forms.RichTextBox rtf = new System.Windows.Forms.RichTextBox();
            rtf.Text = DirtyText;
            return rtf.Rtf;
        }

        public void PrepareData(dynamic ADataset, string AName)
        {
            // Tested Aging Master = 119, Detail = 931 => From 2m ~ 3m 15sec To 50 sec to 1m 15sec

            Tbl.Tables.Add(AName);
            //  Add Columns
            DataColumn col;
            string lFld;
            int I, J, K;
            Dictionary<int, object> dict = new Dictionary<int, object>();            
            dynamic dsFld;
            for (I = 0; (I <= (ADataset.Fields.Count - 1)); I++)
            {
                dict.Add(I, ADataset.Fields.Items(I));
                dsFld = dict[I];
                lFld = dsFld.FieldName;
                col = new DataColumn(lFld);
                Tbl.Tables[AName].Columns.Add(col);
            }

            // Add Rows from the datagridview
            DataRow row;
            J = ADataset.RecordCount();
            K = 0;
            ADataset.DisableControls();
            ADataset.First();
            while (!ADataset.eof)
            {
                K = (K + 1);
                row = Tbl.Tables[AName].Rows.Add();
                for (I = 0; (I
                            <= (ADataset.Fields.Count - 1)); I++)
                {
                    dsFld = dict[I];
                    lFld = dsFld.FieldName();
                    lbTime.Text = "Insert Grid - " + AName + " " 
                                 + K.ToString() + " of " 
                                 + J.ToString() + " - " 
                                 + lFld;
                    row[I] = dsFld.Value;
                }
                ADataset.Next();
            }
        }

        public void SetGridInfo(DataRelation drt)
        {
            // Assign relation to dataset
            Tbl.Relations.Add(drt);

            // Bind the Master to the dataset.
            MasterBS.DataSource = Tbl;
            MasterBS.DataMember = "Master";

            // Bind the Detail to MasterBind
            DetailBS.DataSource = MasterBS;
            DetailBS.DataMember = "relation";

            // Set Datasoure to Grid
            DataGVM.DataSource = MasterBS;
            DataGVD.DataSource = DetailBS;
        }

        public void ClearTables()
        {
            Tbl.Relations.Clear();
            MasterBS.DataMember = "";
            DetailBS.DataMember = "";
            if (Tbl.Tables.Count != 0)
                Tbl.Tables["Detail"].Constraints.Clear();
            Tbl.Tables.Clear();
        }

        private void edDCF_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog()
            {
            Title = "Open DCF File...",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Filter = "DCF File (*.DCF)|*.DCF|All Files (*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                edDCF.Text = fd.FileName;
            }
        }

        private void edDCF2_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog
            {
                Title = "Open DCF File...",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "DCF File (*.DCF)|*.DCF|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                edDCF2.Text = fd.FileName;
            }
        }

        private void edDB_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog
            {
                Title = "Open Firebird File...",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Firebird File (*.FDB)|*.FDB|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                edDB.Text = System.IO.Path.GetFileName(fd.FileName);
            }
        }

        private void edDB2_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog
            {
                Title = "Open Firebird File...",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Firebird File (*.FDB)|*.FDB|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                edDB2.Text = System.IO.Path.GetFileName(fd.FileName);
            }
        }

        private void BtnComplete_Click(object sender, EventArgs e)
        {
            /*This will doing following posting
              01. Cash Sales
              02. Sales Credit Note
              03. Customer Payment With Knock off
              04. Edit Credit Note Posted in Step 02 & Knock Off
              05. Customer Refund to Knock off Credit Note
            */
            dynamic BizObject, lMain, lDetail, lSN;
            DateTime lDate;

            CheckLogin(0);
            lbTime.Text = "Posting Cash Sales...";
            //Begin Looping yr data
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("SL_CS");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data
            lSN = BizObject.DataSets.Find("cdsSerialNumber"); //lSN contains Serial Number data

            
            //Step 4 : Insert Data - Master
            lDate = DateTime.Parse("January 1, 2018");
            BizObject.New();
            lMain.FindField("DocKey").value = -1;
            lMain.FindField("DocNo").AsString = "--IV Test--";
            lMain.FindField("DocDate").value = lDate;
            lMain.FindField("PostDate").value = lDate;
            lMain.FindField("Code").AsString = "300-C0001"; //Customer Account
            lMain.FindField("CompanyName").AsString = "Cash Sales";
            lMain.FindField("Address1").AsString = ""; //Optional
            lMain.FindField("Address2").AsString = ""; //Optional
            lMain.FindField("Address3").AsString = ""; //Optional
            lMain.FindField("Address4").AsString = ""; //Optional
            lMain.FindField("Phone1").AsString = "";   //Optional
            lMain.FindField("Description").AsString = "Sales";

            //Step 5: Insert Data - Detail
            //For Tax Inclusive = True with override Tax Amount
            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("DocKey").value = -1;
            lDetail.FindField("Seq").value = 1;
            lDetail.FindField("Account").AsString = "500-000"; //Sales Account
            lDetail.FindField("Description").AsString = "Sales Item A";
            lDetail.FindField("Description3").AsString = ("Item A Line 1" + ("\r" + "Item A Line 2"));
            lDetail.FindField("Qty").AsFloat = 1;
            lDetail.FindField("Tax").AsString = "SR";
            lDetail.FindField("TaxRate").AsString = "6%";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("UnitPrice").AsFloat = 435;
            lDetail.FindField("Amount").AsFloat = 410.37; //Exclding GST Amt
            lDetail.FindField("TaxAmt").AsFloat = 24.63;

            lDetail.DisableControls();
            lDetail.FindField("TaxInclusive").value = 1;
            lDetail.EnableControls();

            lDetail.Post();

            //For Tax Inclusive = False with override Tax Amount
            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("DocKey").value = -1;
            lDetail.FindField("Seq").value = 2;
            lDetail.FindField("Account").AsString = "500-000";
            lDetail.FindField("Description").AsString = "Sales Item B";
            lDetail.FindField("Qty").AsFloat = 1;
            lDetail.FindField("Tax").AsString = "SR";
            lDetail.FindField("TaxRate").AsString = "6%";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("UnitPrice").AsFloat = 94.43;
            lDetail.FindField("Amount").AsFloat = 94.43;
            lDetail.FindField("TaxAmt").AsFloat = 5.66;
            lDetail.Post();

            //For With Item Code
            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("DocKey").value = -1;
            lDetail.FindField("Seq").value = 3;
            lDetail.FindField("ItemCode").AsString = "ANT";
            lDetail.FindField("Description").AsString = "Sales Item B";
            //lDetail.FindField("Account").AsString     = "500-000"; //If you wanted override the Sales Account Code
            lDetail.FindField("UOM").AsString = "UNIT";
            lDetail.FindField("Qty").AsFloat = 2;            
            //lDetail.FindField("DISC").AsString        = "5%+3"; //Optional(eg 5% plus 3 Discount)
            lDetail.FindField("Tax").AsString = "SR";
            lDetail.FindField("TaxRate").AsString = "6%";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("UnitPrice").AsFloat = 100;
            lDetail.FindField("Amount").AsFloat = 200;
            lDetail.FindField("TaxAmt").AsFloat = 12;
            lDetail.Post();

            //For Item Code with Serial Number
            lDetail.Append();
            lDetail.FindField("DTLKEY").Value = -1;
            lDetail.FindField("DOCKEY").Value = -1;
            lDetail.FindField("SEQ").Value = 4;
            lDetail.FindField("ItemCode").AsString = "SN1";
            lDetail.FindField("ACCOUNT").AsString = "500-000";
            lDetail.FindField("DESCRIPTION").AsString = "Sales Serial Number Item";

            lSN.Append();
            lSN.FindField("SERIALNUMBER").AsString = "SN-00001";
            lSN.Post();

            lSN.Append();
            lSN.FindField("SERIALNUMBER").AsString = "SN-00002";
            lSN.Post();

            lDetail.FindField("UOM").AsString = "UNIT";
            lDetail.FindField("QTY").AsFloat = 2;
            lDetail.FindField("TAX").AsString = "SR";
			lDetail.FindField("TaxRate").AsString = "6%";
            lDetail.FindField("TAXINCLUSIVE").Value = 0;
            lDetail.FindField("UNITPRICE").AsFloat = 94.43;
            lDetail.FindField("TAXAMT").AsFloat = 11.33;
            lDetail.FindField("CHANGED").AsString = "F";
            lDetail.Post();

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
			FreeBiz(BizObject);
            //End Looping yr data

            //Step 7: Payment
            lbTime.Text = "Posting Payment...";
            InsertARPM();

            //Step 8: Credit Note
            lbTime.Text = "Posting Sales Credit Note...";
            InsertSLCN();

            //Step 9: Refund
            lbTime.Text = "Posting Refund...";
            InsertARCF();

            //Step 10 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);            
            Logout();
        }

        public void InsertARPM()
        {
            dynamic BizObject, lMain, lDetail;
            object[] V = new object[2];
            DateTime lDate;
            string lIVNO;

            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_PM");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsKnockOff"); //lDetail contains detail data  

            //Step 4 : Posting
            lDate = DateTime.Parse("January 23, 2018");
            BizObject.New();
            lMain.FindField("DOCKEY").Value = -1;
            lMain.FindField("DocNo").AsString = "--PM Test--";
            lMain.FindField("CODE").AsString = "300-C0001"; //Customer Account
            lMain.FindField("DocDate").Value = lDate;
            lMain.FindField("PostDate").Value = lDate;
            lMain.FindField("Description").AsString = "Payment for A/c";
            lMain.FindField("PaymentMethod").AsString = "320-000"; //Bank or Cash Account
            lMain.FindField("ChequeNumber").AsString = "";
            lMain.FindField("BankCharge").AsFloat = 0;
            lMain.FindField("DocAmt").AsFloat = 200.00;
            lMain.FindField("Cancelled").AsString = "F";
            lMain.Post();

            //Step 5: Knock Off IV
            lIVNO = "--IV Test--";
            V[0] = "IV";
            V[1] = lIVNO;

            if (lDetail.Locate("DocType;DocNo", V, false, false))
            {
                lDetail.Edit();
                lDetail.FindField("KOAmt").AsFloat = 147.09; //Partial Knock off
                lDetail.FindField("KnockOff").AsString = "T";
                lDetail.Post();
            }

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
			FreeBiz(BizObject);
        }

        public void InsertSLCN()
        {
            dynamic BizObject, lMain, lDetail;
            DateTime lDate;
            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("SL_CN");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  

            //Step 4 : Insert Data - Master
            lDate = DateTime.Parse("January 24, 2018");
            BizObject.New();
            lMain.FindField("DocKey").value = -1;
            lMain.FindField("DocNo").AsString = "--CN Test--";
            lMain.FindField("DocDate").value = lDate;
            lMain.FindField("PostDate").value = lDate;
            lMain.FindField("Code").AsString = "300-C0001";
            lMain.FindField("CompanyName").AsString = "Cash Sales";
            lMain.FindField("Address1").AsString = "";
            lMain.FindField("Address2").AsString = "";
            lMain.FindField("Address3").AsString = "";
            lMain.FindField("Address4").AsString = "";
            lMain.FindField("Phone1").AsString = "";
            lMain.FindField("Description").AsString = "Sales Returned";

            //For With Item Code
            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("DocKey").value = -1;
            lDetail.FindField("ItemCode").AsString = "ANT";
            lDetail.FindField("Description").AsString = "Sales Item B";
            lDetail.FindField("Description2").AsString = "Product Spoil"; //Reason
            lDetail.FindField("Remark1").AsString = "--IV Test--";   //Invoice No
            lDetail.FindField("Remark2").AsString = "01 Jan 2017";   //Invoice Date
            lDetail.FindField("Qty").AsFloat = 1;
            lDetail.FindField("Tax").AsString = "SR";
            lDetail.FindField("TaxRate").AsString = "6%";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("UnitPrice").AsFloat = 100;
            lDetail.FindField("Amount").AsFloat = 100;
            lDetail.FindField("TaxAmt").AsFloat = 6;
            lDetail.Post();

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
			FreeBiz(BizObject);

            //Step 7: Knock Off Invoice
            lbTime.Text = "Posting Credit Note - Knock Invoice...";
            KnockIVCN();
        }

        public void KnockIVCN()
        {
            dynamic BizObject, lMain, lDetail, lDocKey;
            object[] V = new object[2];
            string lDocNo, lIVNO;

            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_CN");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet"); //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsKnockOff"); //lDetail contains Knock off data  

            //Step 4 : Find CN Number
            lDocNo = "--CN Test--";
            lDocKey = BizObject.FindKeyByRef("DocNo", lDocNo);
            BizObject.Params.Find("DocKey").Value = lDocKey;

            if (!Convert.IsDBNull(lDocKey))
            {
                BizObject.Open();
                BizObject.Edit();
                lMain.Edit();
                //Step 5: Knock Off IV
                lIVNO = "--IV Test--";
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 100; //Partial Knock off
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }

                //Step 6: Save Document
                BizObject.Save();
                BizObject.Close();
				FreeBiz(BizObject);
            }
        }

        public void InsertARCF()
        {
            dynamic BizObject, lMain, lDetail;
            object[] V = new object[2];
            DateTime lDate;
            string lIVNO;

            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_CF");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsKnockOff"); //lDetail contains detail data  

            //Step 4 : Posting
            lDate = DateTime.Parse("January 23, 2018");
            BizObject.New();
            lMain.FindField("DOCKEY").Value = -1;
            lMain.FindField("DocNo").AsString = "--CF Test--";
            lMain.FindField("CODE").AsString = "300-C0001"; //Customer Account
            lMain.FindField("DocDate").Value = lDate;
            lMain.FindField("PostDate").Value = lDate;
            lMain.FindField("Description").AsString = "Payment for A/c";
            lMain.FindField("PaymentMethod").AsString = "320-000"; //Bank or Cash Account
            lMain.FindField("ChequeNumber").AsString = "";
            lMain.FindField("BankCharge").AsFloat = 0;
            lMain.FindField("DocAmt").AsFloat = 6.0;
            lMain.FindField("Cancelled").AsString = "F";

            //Step 5: Knock Off CN
            lIVNO = "--CN Test--";
            V[0] = "CN";
            V[1] = lIVNO;

            if (lDetail.Locate("DocType;DocNo", V, false, false))
            {
                lDetail.Edit();
                lDetail.FindField("KOAmt").AsFloat = 4; //Partial Knock off
                lDetail.FindField("KnockOff").AsString = "T";
                lDetail.Post();
            }

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
			FreeBiz(BizObject);
        }

        dynamic lMain, lDtl, BizObject, lQty,
                lFld_MDockey, lFld_DocNo, lFld_Code, lFld_DocDate, lFld_PostDate, lFld_TaxDate,
                lFld_Desc, lFld_Cancelled,
                lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_ItemCode, lFld_Acc, lFld_Qty, lFld_Tax,
                lFld_TaxRate, lFld_TaxInc, lFld_UnitPrice, lFld_TaxAmt;

        private void btnPost1_Click(object sender, EventArgs e)
        {
            int I, J, R;
            DateTime lDate = new DateTime(DateTime.Now.Year, 1, 1);
            Random IP;

            for (I = 1; (I <= edRecord.Value); I++)
            {
                if (I == 1)
                {
                    CheckLogin(0);

                    BizObject = ComServer.BizObjects.Find(edBizType.Text);
                    lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data
                    lDtl = BizObject.DataSets.Find("cdsDocDetail");  //lDetail contains detail data

                    lFld_MDockey = lMain.FindField("DOCKEY");
                    lFld_DocNo = lMain.FindField("DocNo");
                    lFld_Code = lMain.FindField("CODE");
                    lFld_DocDate = lMain.FindField("DocDate");
                    lFld_PostDate = lMain.FindField("PostDate");
                    lFld_TaxDate = lMain.FindField("TAXDATE");
                    lFld_Desc = lMain.FindField("Description");
                    lFld_Cancelled = lMain.FindField("Cancelled");

                    lFld_DDockey = lDtl.FindField("DOCKEY");
                    lFld_DtlKey = lDtl.FindField("DTLKEY");
                    lFld_Seq = lDtl.FindField("SEQ");
                    lFld_ItemCode = lDtl.FindField("ItemCode");
                    lFld_Acc = lDtl.FindField("Account");
                    lFld_Qty = lDtl.FindField("QTY");
                    lFld_Tax = lDtl.FindField("TAX");
                    lFld_TaxRate = lDtl.FindField("TAXRATE");
                    lFld_TaxInc = lDtl.FindField("TAXINCLUSIVE");
                    lFld_UnitPrice = lDtl.FindField("UNITPRICE");
                    lFld_TaxAmt = lDtl.FindField("TaxAmt");
                }
                lbTime.Text = "Posting Record No : " + I;
                BizObject.New();
                //Begin Append Master
                lFld_MDockey.Value = -1;
                lFld_DocNo.AsString = String.Format(edFmt.Text, I);

                IP = new Random();
                R = IP.Next(0, edCoCode.Lines.Count());
                lFld_Code.AsString = edCoCode.Lines.GetValue(R);

                if (I >= 10)
                    lDate = lDate.AddDays(I / (double)10);
                lFld_DocDate.Value = lDate;
                lFld_PostDate.Value = lDate;
                lFld_TaxDate.Value = lDate;
                lFld_Desc.AsString = "Sales";
                lFld_Cancelled.Value = "F";
                lMain.Post();

                // Begin Append Detail
                for (J = 1; J <= edRecordItem.Value; J++)
                {
                    lDtl.Append();
                    lFld_DtlKey.Value = -1;
                    lFld_DDockey.Value = -1;
                    lFld_Seq.Value = J;

                    IP = new Random();
                    R = IP.Next(0, edItem.Lines.Count());
                    lFld_ItemCode.AsString = edItem.Lines.GetValue(R);

                    IP = new Random();
                    lQty = IP.Next(1, 10);

                    // lFld_Acc.AsString = "500-000"; //Sales Account Code  & can ignore if had itemcode
                    lFld_Qty.AsFloat = lQty;

                    if (edWithTax.Checked)
                    {
                        lFld_Tax.AsString = "SR";
                        lFld_TaxRate.AsString = "6%";
                        lFld_TaxInc.Value = 0;
                    }
                    else
                    {
                        lFld_Tax.AsString = "";
                        lFld_TaxRate.AsString = "";
                        lFld_TaxInc.Value = 0;
                    }

                    lFld_UnitPrice.AsFloat = 100;
                    if (edWithTax.Checked)
                        lFld_TaxAmt.AsFloat = 0.06 * (lQty * 100); // TaxRate * (Qty * UnitPrice Excluding Tax)
                    else
                        lFld_TaxAmt.AsFloat = 0;
                    lDtl.Post();
                }
                BizObject.Save();
                BizObject.Close();
            }
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnDelSKU_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lDocKey;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("ST_ITEM");

            //Step 3: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);

            //Step 4 : Insert or Update
            if (Convert.IsDBNull(lDocKey) != null)
            {//Edit Data if found
                BizObject.Params.Find("Dockey").Value = lDocKey;
                BizObject.Open();
                BizObject.Delete();
            }
            //Step 5: Save & Close
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Step 6 : Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void btnSLCSDel_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDocKey;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("SL_CS");
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            //Step 3: Search
            lDocKey = BizObject.FindKeyByRef("DocNo", edDocNo.Text);

            //Step 4 : Insert or Update
            if (Convert.IsDBNull(lDocKey) != null)
            {//Edit Data if found
                BizObject.Params.Find("Dockey").Value = lDocKey;
                BizObject.Open();

                lMain.Edit();
                lMain.FindField("P_PAYMENTMETHOD").AsString = ""; // Make sure no payment
                BizObject.Save();

                BizObject.Delete();
            }
            //Step 5: Save & Close
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Step 6 : Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnARCustomer_Click(object sender, EventArgs e)
        {
            dynamic lMain, lDtl;
            string lSQL;
            DataColumn tblC1, tblC2;
            DataRelation drt;            

            // Step 1: Login
            CheckLogin();

            lSQL = "SELECT * FROM AR_CUSTOMER  ";
            lSQL = lSQL + "WHERE STATUS='A' ";

            lMain = ComServer.DBManager.NewDataSet(lSQL);

            lSQL = "SELECT * FROM AR_CUSTOMERBRANCH ";
            lSQL = lSQL + "WHERE CODE IN (SELECT CODE FROM AR_CUSTOMER ";
            lSQL = lSQL + "               WHERE STATUS='A') ";

            lDtl = ComServer.DBManager.NewDataSet(lSQL);

            PrepareData(lMain, "Master");
            PrepareData(lDtl, "Detail");

            tblC1 = Tbl.Tables["Master"].Columns["Code"];
            tblC2 = Tbl.Tables["Detail"].Columns["Code"];
            drt = new DataRelation("relation", tblC1, tblC2);

            // Step 7 : Set Grid Info
            SetGridInfo(drt);

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done
            FreeBiz(lMain);
            FreeBiz(lDtl);
            Logout();
        }

        private void BtnAddCust_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDtl, lDocKey;
            string V;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_Customer");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data
            lDtl = BizObject.DataSets.Find("cdsBranch");        //lDtl contains UOM data

            //Step 4: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);

            //Step 5 : Insert or Update
            if (Convert.IsDBNull(lDocKey))
            {
                BizObject.New();
                lMain.FindField("CODE").value = edCode.Text;
                lMain.FindField("CompanyName").value = edDesc.Text;

                lDtl.Edit(); //For 1St Branch
                lDtl.FindField("BranchName").AsString = "BILLING";
                lDtl.FindField("Address1").AsString = "Address1";
                lDtl.FindField("Address2").AsString = "Address2";
                lDtl.FindField("Address3").AsString = "Address3";
                lDtl.FindField("Address4").AsString = "Address4";
                lDtl.FindField("Attention").AsString = "Attention";
                lDtl.FindField("Phone1").AsString = "Phone1";
                lDtl.FindField("Fax1").AsString = "Fax1";
                lDtl.FindField("Email").AsString = "EmailAddress";
                lDtl.Post();

                lDtl.Append(); //For 2nd Branch
                lDtl.FindField("BranchName").AsString = "Branch1";
                lDtl.FindField("Address1").AsString = "DAddress1";
                lDtl.FindField("Address2").AsString = "DAddress2";
                lDtl.FindField("Address3").AsString = "DAddress3";
                lDtl.FindField("Address4").AsString = "DAddress4";
                lDtl.FindField("Attention").AsString = "DAttention";
                lDtl.FindField("Phone1").AsString = "DPhone1";
                lDtl.FindField("Fax1").AsString = "DFax1";
                lDtl.FindField("Email").AsString = "DEmailAddress";
                lDtl.Post();
            }
            else
            {//Edit Data if found
                BizObject.Params.Find("Code").Value = lDocKey;
                BizObject.Open();
                BizObject.Edit();
                lMain.FindField("CompanyName").value = edDesc.Text;

                while (lDtl.RecordCount != 0)
                {
                    lDtl.First();
                    lDtl.Delete();
                }
                lDtl.Append();
                lDtl.FindField("BRANCHTYPE").AsString = "B";
                lDtl.FindField("BranchName").AsString = "BILLING";
                lDtl.FindField("Address1").AsString = "Address1-Changed";
                lDtl.FindField("Address2").AsString = "Address2-Changed";
                lDtl.FindField("Address3").AsString = "Address3-Changed";
                lDtl.FindField("Address4").AsString = "Address4-Changed";
                lDtl.FindField("Attention").AsString = "Attention-Changed";
                lDtl.FindField("Phone1").AsString = "Phone1-Changed";
                lDtl.FindField("Fax1").AsString = "Fax1-Changed";
                lDtl.FindField("Email").AsString = "EmailAddress-Changed";
                lDtl.Post();

                lDtl.Append();
                lDtl.FindField("BRANCHTYPE").AsString = "D";
                lDtl.FindField("BranchName").AsString = "SETIA ALAM";
                lDtl.FindField("Address1").AsString = "Address1-SA1";
                lDtl.FindField("Address2").AsString = "Address2-SA2";
                lDtl.FindField("Address3").AsString = "Address3-SA3";
                lDtl.FindField("Address4").AsString = "Address4-SA4";
                lDtl.FindField("Attention").AsString = "Attention-Branch";
                lDtl.FindField("Phone1").AsString = "Phone1-Branch";
                lDtl.FindField("Fax1").AsString = "Fax1-Branch";
                lDtl.FindField("Email").AsString = "EmailAddress-Branch";
                lDtl.Post();

                //V = "B";
                //if (lDtl.Locate("BRANCHTYPE", V, false, false))
                //{
                //lDtl.Edit();
                //lDtl.FindField("Address1").AsString = "Address1-Changed";
                //lDtl.FindField("Address2").AsString = "Address2-Changed";
                //lDtl.FindField("Address3").AsString = "Address3-Changed";
                //lDtl.FindField("Address4").AsString = "Address4-Changed";
                //lDtl.FindField("Attention").AsString = "Attention-Changed";
                //lDtl.FindField("Phone1").AsString = "Phone1-Changed";
                //lDtl.FindField("Fax1").AsString = "Fax1-Changed";
                //lDtl.FindField("Email").AsString = "EmailAddress-Changed";
                //lDtl.Post();
                //}                

                //V = "Branch1";
                //if (lDtl.Locate("BranchName", V, false, false))
                //{
                //lDtl.Edit();
                //lDtl.FindField("Address1").AsString = "DAddress1-Changed";
                //lDtl.FindField("Address2").AsString = "DAddress2-Changed";
                //lDtl.FindField("Address3").AsString = "DAddress3-Changed";
                //lDtl.FindField("Address4").AsString = "DAddress4-Changed";
                //lDtl.FindField("Attention").AsString = "DAttention-Changed";
                //lDtl.FindField("Phone1").AsString = "DPhone1-Changed";
                //lDtl.FindField("Fax1").AsString = "DFax1-Changed";
                //lDtl.FindField("Email").AsString = "DEmailAddress-Changed";
                //lDtl.Post();
                //}
            }
            //Step 6: Save & Close
            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Step 7 : Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnDelCust_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lDocKey;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_Customer");

            //Step 3: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);

            //Step 4 : Insert or Update
            if (Convert.IsDBNull(lDocKey) != null)
            {//Edit Data if found
                BizObject.Params.Find("Code").Value = lDocKey;
                BizObject.Open();
                BizObject.Delete();
            }
            //Step 5: Save & Close
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Step 6 : Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnLedger_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet;

            // Step 1: Create Com Server object
            CheckLogin();

            // Step 2: Get DocNo
            lSQL = "SELECT CODE, DOCDATE, POSTDATE, DESCRIPTION, DESCRIPTION2,";
            lSQL = lSQL + "LOCALDR, LOCALCR, REF1, REF2 FROM GL_TRANS ";
            lSQL = lSQL + "WHERE CANCELLED='F' ";
            lSQL = lSQL + "AND POSTDATE BETWEEN '01 JAN 2018' ";
            lSQL = lSQL + "AND '31 DEC 2018' ";

            if (edCode.Text != "")
            {
                // if wanted filter by Account Code
                lSQL = lSQL + "AND CODE=" + QuotedStr(edCode.Text);                
            }

            lSQL = lSQL + " ORDER BY CODE, POSTDATE ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            if (lDataSet.RecordCount > 0)
            {
                Tbl.Tables.Clear();
                PrepareData(lDataSet, "Master");

               //Bind the Master to the dataset.
                MasterBS.DataSource = Tbl;
                MasterBS.DataMember = "Master";

                //Set Datasoure to Grid
                DataGVM.DataSource = MasterBS;
                FreeBiz(lDataSet);
                Logout();
            }
            else
                MessageBox.Show("Record Not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void btnARIVList_Click(object sender, EventArgs e)
        {
            dynamic RptObject, lDateFrom, lDateTo, lDataSet1, lDataSet2;
            DataColumn tblC1, tblC2;
            DataRelation drt;

            // Step 1: Create Com Server object
            CheckLogin();
            // Step 2: Find and Create the Report Objects
            RptObject = ComServer.RptObjects.Find("Customer.IV.RO");
            // Step 3: Spool parameters
            //RptObject.Params.Find("AgentData").Value              = //Not use if AllAgent is true
            //RptObject.Params.Find("CompanyCategoryData").Value    = //Not use if AllCompanyCategory is true
            RptObject.Params.Find("AllAgent").Value = true;
            RptObject.Params.Find("AllCompanyCategory").Value = true;
            RptObject.Params.Find("AllArea").Value = true;
            RptObject.Params.Find("AllCompany").Value = false;
            RptObject.Params.Find("AllCurrency").Value = true;
            RptObject.Params.Find("AllDocument").Value = true;
            RptObject.Params.Find("AllPaymentMethod").Value = true; 
            //RptObject.Params.Find("AreaData").Value = //Not use if AllArea is true
            RptObject.Params.Find("CompanyData").Value = "300-A0003";
            //RptObject.Params.Find("CurrencyData").Value = //Not use if AllCurrency is true
            lDateFrom = DateTime.Parse("January 01, 2019");
            lDateTo = DateTime.Parse("December 31, 2019");

            RptObject.Params.Find("DateFrom").Value = lDateFrom;
            RptObject.Params.Find("DateTo").Value = lDateTo;
            //RptObject.Params.Find("DocumentData").Value = //Not use if AllDocument is true
            //RptObject.Params.Find("GroupBy").Value = //if you wanted to grouping
            RptObject.Params.Find("IncludeCancelled").Value = false;
            //RptObject.Params.Find("PaymentMethodData").Value = //Not use if AllPaymentMethod is true
            RptObject.Params.Find("PrintDocumentStyle").Value = false;
            RptObject.Params.Find("SelectDate").Value = true;
            RptObject.Params.Find("ShowUnappliedAmountOnly").Value = false;
            RptObject.Params.Find("SortBy").Value = "DocDate;DocNo;Code";
            RptObject.Params.Find("AllDocProject").Value = true;
            RptObject.Params.Find("AllItemProject").Value = true;
            //RptObject.Params.Find("DocProjectData").Value = //Not use if AllDocProject is true
            //RptObject.Params.Find("ItemProjectData").Value = //Not use if AllItemProject is true

            //Step 4: Perform Report calculation 
            RptObject.CalculateReport();

            lDataSet1 = RptObject.DataSets.Find("cdsMain");
            lDataSet2 = RptObject.DataSets.Find("cdsDocDetail");

            // Step 5 : Convert to Dataset
            ClearTables();

            PrepareData(lDataSet1, "Master");
            PrepareData(lDataSet2, "Detail");

            // Step 6 : Get Relation column
            tblC1 = Tbl.Tables["Master"].Columns["Dockey"];
            tblC2 = Tbl.Tables["Detail"].Columns["Dockey"];
            drt = new DataRelation("relation", tblC1, tblC2);


            // Step 7 : Set Grid Info
            SetGridInfo(drt);
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            Logout();
            RptObject = null;
        }

        private void btnSNBal_Click(object sender, EventArgs e)
        {
            dynamic lDataSet;
            string lSQL;

            // Step 1: Login
            CheckLogin();


            lSQL = "SELECT ItemCode, Location, Batch, SerialNumber, SUM(Qty) Qty  FROM ST_TR_SN ";

            if (edCode.Text != "")
            {
                lSQL = lSQL + "WHERE ITEMCODE=" + QuotedStr(edCode.Text);
            }

            lSQL = lSQL + " GROUP BY ItemCode, Location, Batch, SerialNumber";
            lSQL = lSQL + " HAVING SUM(Qty) > 0";
            
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            Tbl.Tables.Clear();
            PrepareData(lDataSet, "Master");

            // Bind the Master to the dataset.
            MasterBS.DataSource = Tbl;
            MasterBS.DataMember = "Master";

            // Set Datasoure to Grid
            DataGVM.DataSource = MasterBS;

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            FreeBiz(lDataSet);
            Logout();
        }

        private void BtnARDP_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain;
            DateTime lDate;

            CheckLogin(0);
            lbTime.Text = "Posting AR Deposit...";
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_DP");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data

            //Begin Looping yr data
            //Step 4 : Insert Data - Master
            lDate = DateTime.Parse("January 1, 2019");
            BizObject.New();
            lMain.FindField("DocKey").value = -1;
            lMain.FindField("DocNo").AsString = "--DP Test--";
            lMain.FindField("DocDate").value = lDate;
            lMain.FindField("PostDate").value = lDate;
            lMain.FindField("TaxDate").value = lDate;
            lMain.FindField("Code").AsString = "300-C0001"; //Customer Account
            lMain.FindField("DEPOSITACCOUNT").AsString = "PREPAYMENT"; //Prepayment Account
            lMain.FindField("PaymentMethod").AsString = "320-000"; //Bank or Cash account
            lMain.FindField("Description").AsString = "Deposit For Account";
            lMain.FindField("ChequeNumber").AsString = "";
            lMain.FindField("BankCharge").Value = 0.00;
            lMain.FindField("DocAmt").Value = 1000.00;


            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
            //End Looping yr data

            //Step 10 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnDP2PM_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail, lDataSet;
            object[] V = new object[2];
            DateTime lDate;
            string lIVNO, lSQL;

            // Create Com Server object
            CheckLogin();

            lbTime.Text = "Check Deposit Number...";
            // Query Data
            lSQL = "SELECT DOCKEY FROM AR_DP ";
            lSQL = lSQL + "WHERE DOCNO='--DP Test--' ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            if (lDataSet.RecordCount > 0)
            {
                lbTime.Text = "Posting AR Payment...";
                //Step 2: Find and Create the Biz Objects
                BizObject = ComServer.BizObjects.Find("AR_PM");

                //Step 3: Set Dataset
                lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data
                lDetail = BizObject.DataSets.Find("cdsKnockOff"); //lDetail contains detail data  

                //Step 4 : Posting
                lDate = DateTime.Parse("January 23, 2019");
                BizObject.New();
                lMain.FindField("DOCKEY").Value = -1;
                lMain.FindField("DocNo").AsString = "--DP2PM Test--";
                lMain.FindField("CODE").AsString = "300-C0001"; //Customer Account
                lMain.FindField("DocDate").Value = lDate;
                lMain.FindField("PostDate").Value = lDate;
                lMain.FindField("Description").AsString = "Payment for A/c";
                lMain.FindField("PaymentMethod").AsString = "PREPAYMENT"; //Prepayment Account
                lMain.FindField("DocAmt").AsFloat = 200.00;
                lMain.FindField("FROMDOCTYPE").AsString = "DP";
                lMain.FindField("FROMDOCKEY").Value = lDataSet.FindField("Dockey").AsFloat; //Transfer from Deposit
                lMain.FindField("Cancelled").AsString = "F";

                //Step 5: Knock Off IV
                lIVNO = "--IV Test--";
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 147.09; //Partial Knock off
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }

                //Step 6: Save Document
                BizObject.Save();
                BizObject.Close();
                FreeBiz(BizObject);
            }
            //Step 7 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);        
            Logout();
        }

        private void BtnDPRefund_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDataSet;
            DateTime lDate;
            string lSQL;

            // Create Com Server object
            CheckLogin();

            lbTime.Text = "Check Deposit Number...";
            // Query Data
            lSQL = "SELECT DOCKEY FROM AR_DP ";
            lSQL = lSQL + "WHERE DOCNO='--DP Test--' ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            if (lDataSet.RecordCount > 0)
            {
                lbTime.Text = "Posting Deposit Refund...";
                //Step 2: Find and Create the Biz Objects
                BizObject = ComServer.BizObjects.Find("AR_DPDTL_REFUND");

                //Step 3: Set Dataset
                lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data

                //Step 4 : Posting
                lDate = DateTime.Parse("January 23, 2019");
                BizObject.New();
                lMain.FindField("DocKey").value = lDataSet.FindField("Dockey").AsFloat;
                lMain.FindField("Account").AsString = "320-000"; //Bank or Cash account
                lMain.FindField("DocDate").value = lDate;
                lMain.FindField("PostDate").value = lDate;
                lMain.FindField("Description").AsString = "Deposit Refund";
                lMain.FindField("ChequeNumber").AsString = "";
                lMain.FindField("BankCharge").Value = 0.00;
                lMain.FindField("PAYMENTAMOUNT").Value = 500.00;

                //Step 5: Save Document
                BizObject.Save();
                BizObject.Close();
                FreeBiz(BizObject);
            }
            //Step 6 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logout();
        }

        private void BtnDPForfeit_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDataSet;
            DateTime lDate;
            string lSQL;

            // Create Com Server object
            CheckLogin();

            lbTime.Text = "Check Deposit Number...";
            // Query Data
            lSQL = "SELECT DOCKEY FROM AR_DP ";
            lSQL = lSQL + "WHERE DOCNO='--DP Test--' ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            if (lDataSet.RecordCount > 0)
            {
                lbTime.Text = "Posting Deposit Forfeit...";
                //Step 2: Find and Create the Biz Objects
                BizObject = ComServer.BizObjects.Find("AR_DPDTL_FORFEIT");

                //Step 3: Set Dataset
                lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data

                //Step 4 : Posting
                lDate = DateTime.Parse("January 23, 2019");
                BizObject.New();
                lMain.FindField("DocKey").value = lDataSet.FindField("Dockey").AsFloat;
                lMain.FindField("Account").AsString = "532-000"; //Forfeit account
                lMain.FindField("DocDate").value = lDate;
                lMain.FindField("PostDate").value = lDate;
                lMain.FindField("Description").AsString = "Deposit Forfeit";
                lMain.FindField("ChequeNumber").AsString = "";
                lMain.FindField("BankCharge").Value = 0.00;
                lMain.FindField("Amount").Value = 150.00;

                //Step 5: Save Document
                BizObject.Save();
                BizObject.Close();
                FreeBiz(BizObject);
            }
            //Step 6 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logout();
        }

        private void BtnDO2IV_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail, lxFer,
                    lFld_MDockey, lFld_DocNo,
                    lFld_Code, lFld_DocDate, lFld_PostDate, lFld_CoName, lFld_Desc,
                    lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_ItemCode, lFld_DDesc, 
                    lFld_FromDocType, lFld_FromDockey, lFld_FromDtlkey,
                    lFld_Qty, lFld_UOM, lFld_Tax, lFld_TaxRate, lFld_TaxInc,
                    lFld_UPrice, lFld_Amt, lFld_TaxAmt;

            string lSQL;
            DateTime lDate;

            CheckLogin(0);
            lbTime.Text = "Posting Delivery Order...";
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("SL_DO");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  

            lFld_MDockey = lMain.FindField("DocKey");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Code = lMain.FindField("Code");
            lFld_CoName = lMain.FindField("CompanyName");
            lFld_Desc = lMain.FindField("Description");

            lFld_DDockey = lDetail.FindField("DocKey");
            lFld_DtlKey = lDetail.FindField("DtlKey");
            lFld_Seq = lDetail.FindField("Seq");
            lFld_ItemCode = lDetail.FindField("ItemCode");
            lFld_DDesc = lDetail.FindField("Description");
            lFld_Qty = lDetail.FindField("Qty");
            lFld_UOM = lDetail.FindField("UOM");
            lFld_Tax = lDetail.FindField("Tax");
            lFld_TaxRate = lDetail.FindField("TaxRate");
            lFld_TaxInc = lDetail.FindField("TaxInclusive");
            lFld_UPrice = lDetail.FindField("UnitPrice");
            lFld_Amt = lDetail.FindField("Amount");
            lFld_TaxAmt = lDetail.FindField("TaxAmt");

            //Step 4 : Insert Data - Master
            lDate = DateTime.Parse("January 1, 2019");
            BizObject.New();
            lFld_MDockey.value = -1;
            lFld_DocNo.AsString = "--DO Test--";
            lFld_DocDate.value = lDate;
            lFld_PostDate.value = lDate;
            lFld_Code.AsString = "300-C0001"; //Customer Account
            lFld_CoName.AsString = "Cash Sales";
            //lMain.FindField("Address1").AsString = ""; //Optional
            //lMain.FindField("Address2").AsString = ""; //Optional
            //lMain.FindField("Address3").AsString = ""; //Optional
            //lMain.FindField("Address4").AsString = ""; //Optional
            //lMain.FindField("Phone1").AsString = "";   //Optional
            lFld_Desc.AsString = "Delivery Order";

            //Step 5: Insert Data - Detail
            lDetail.Append();
            lFld_DtlKey.value = -1;
            lFld_DDockey.value = -1;
            lFld_Seq.value = 1;
            lFld_ItemCode.AsString = "ANT";
            lFld_DDesc.AsString = "Sales Item A";
            //lDetail.FindField("Account").AsString     = "500-000"; //If you wanted override the Sales Account Code            
            lFld_UOM.AsString = "UNIT";
            lFld_Qty.AsFloat = 2;
            //lDetail.FindField("DISC").AsString        = "5%+3"; //Optional(eg 5% plus 3 Discount)
            lFld_Tax.AsString = "SV";
            lFld_TaxRate.AsString = "6%";
            lFld_TaxInc.value = 0;
            lFld_UPrice.AsFloat = 100;
            lFld_Amt.AsFloat = 200;
            lFld_TaxAmt.AsFloat = 12;
            lDetail.Post();

            lDetail.Append();
            lFld_DtlKey.value = -1;
            lFld_DDockey.value = -1;
            lFld_Seq.value = 2;
            lFld_ItemCode.AsString = "COVER";
            lFld_DDesc.AsString = "Sales Item B";            
            lFld_UOM.AsString = "UNIT";
            lFld_Qty.AsFloat = 3;
            lFld_Tax.AsString = "SV";
            lFld_TaxRate.AsString = "6%";
            lFld_TaxInc.value = 0;
            lFld_UPrice.AsFloat = 10;
            lFld_Amt.AsFloat = 30;
            lFld_TaxAmt.AsFloat = 1.80;
            lDetail.Post();

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();

            // Check Is Transferred or not
            lbTime.Text = "Check Transfer Status...";
            lSQL = "SELECT DocKey FROM SL_IVDTL ";
            lSQL = lSQL + "WHERE FromDocKey IN (SELECT DocKey FROM SL_DO ";
            lSQL = lSQL + "WHERE DocNo='--DO Test--' ";
            lSQL = lSQL + "AND Cancelled='F') ";
            lSQL = lSQL + "AND FromDocType='DO' ";
            lxFer = ComServer.DBManager.NewDataSet(lSQL);

            if (lxFer.RecordCount == 0)
            {
                //Get DO Information
                lSQL = "SELECT * FROM SL_DODTL ";
                lSQL = lSQL + "WHERE DocKey IN (SELECT DocKey FROM SL_DO ";
                lSQL = lSQL + "WHERE DocNo='--DO Test--') ";
                lSQL = lSQL + "ORDER BY SEQ ";
                lxFer = ComServer.DBManager.NewDataSet(lSQL);

                lbTime.Text = "Posting Invoice...";
                //Find and Create the Biz Objects
                BizObject = ComServer.BizObjects.Find("SL_IV");

                //Set Dataset
                lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
                lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  

                lFld_MDockey = lMain.FindField("DocKey");
                lFld_DocNo = lMain.FindField("DocNo");
                lFld_DocDate = lMain.FindField("DocDate");
                lFld_PostDate = lMain.FindField("PostDate");
                lFld_Code = lMain.FindField("Code");
                lFld_CoName = lMain.FindField("CompanyName");
                lFld_Desc = lMain.FindField("Description");

                lFld_DDockey = lDetail.FindField("DocKey");
                lFld_DtlKey = lDetail.FindField("DtlKey");
                lFld_Seq = lDetail.FindField("Seq");
                lFld_ItemCode = lDetail.FindField("ItemCode");
                lFld_DDesc = lDetail.FindField("Description");
                lFld_Qty = lDetail.FindField("Qty");
                lFld_UOM = lDetail.FindField("UOM");
                lFld_Tax = lDetail.FindField("Tax");
                lFld_TaxRate = lDetail.FindField("TaxRate");
                lFld_TaxInc = lDetail.FindField("TaxInclusive");
                lFld_UPrice = lDetail.FindField("UnitPrice");
                lFld_Amt = lDetail.FindField("Amount");
                lFld_TaxAmt = lDetail.FindField("TaxAmt");
                lFld_FromDocType = lDetail.FindField("FromDocType");
                lFld_FromDockey = lDetail.FindField("FromDockey");
                lFld_FromDtlkey = lDetail.FindField("FromDtlKey");
           

                lbTime.Text = "Transfer DO to IV...";
                lDate = DateTime.Parse("January 10, 2019");
                BizObject.New();
                lFld_MDockey.value = -1;
                lFld_DocNo.AsString = "--IV Test--";
                lFld_DocDate.value = lDate;
                lFld_PostDate.value = lDate;
                lFld_Code.AsString = "300-C0001"; //Customer Account
                lFld_CoName.AsString = "Cash Sales";
                //lMain.FindField("Address1").AsString = ""; //Optional
                //lMain.FindField("Address2").AsString = ""; //Optional
                //lMain.FindField("Address3").AsString = ""; //Optional
                //lMain.FindField("Address4").AsString = ""; //Optional
                //lMain.FindField("Phone1").AsString = "";   //Optional
                lFld_Desc.AsString = "Invoice";

                lxFer.First();
                while ((!lxFer.Eof))
                {
                    lDetail.Append();
                    lFld_DtlKey.value = -1;
                    lFld_DDockey.value = -1;
                    lFld_Seq.value = lxFer.FindField("Seq").Value;
                    lFld_ItemCode.AsString = lxFer.FindField("ItemCode").AsString;
                    lFld_DDesc.AsString = lxFer.FindField("Description").AsString;
                    lFld_UOM.AsString = lxFer.FindField("UOM").AsString;
                    lFld_Qty.AsFloat = lxFer.FindField("Qty").AsFloat;
                    lFld_Tax.AsString = lxFer.FindField("Tax").AsString;
                    lFld_TaxRate.AsString = lxFer.FindField("TaxRate").AsString;
                    lFld_TaxInc.value = lxFer.FindField("TaxInclusive").Value;
                    lFld_UPrice.AsFloat = lxFer.FindField("UnitPrice").AsFloat;
                    lFld_Amt.AsFloat = lxFer.FindField("Amount").AsFloat;
                    lFld_TaxAmt.AsFloat = lxFer.FindField("TaxAmt").AsFloat;
                    lFld_FromDocType.AsString = "DO";
                    lFld_FromDockey.Value = lxFer.FindField("DocKey").Value;
                    lFld_FromDtlkey.Value = lxFer.FindField("DtlKey").Value;
                    lDetail.Post();
                    lxFer.Next();
                }
                BizObject.Save();
                BizObject.Close();
                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            FreeBiz(BizObject);
            FreeBiz(lxFer);
            Logout();
            lbTime.Text = "";
        }

        private void BtnSTAJ_Click(object sender, EventArgs e)
        {
            dynamic lDate, lMain, lDtl, BizObject, lFld_MDockey, lFld_DocNo,
                    lFld_DocDate, lFld_PostDate, lFld_Desc,
                    lFld_Cancelled, lFld_DDockey, lFld_DtlKey,
                    lFld_Seq, lFld_ItemCode, lFld_Qty, lFld_UOM, 
					lFld_DDesc, lFld_UCost;

            CheckLogin(0);

            BizObject = ComServer.BizObjects.Find("ST_AJ");
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDtl = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data

            lFld_MDockey = lMain.FindField("DOCKEY");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Desc = lMain.FindField("Description");
            lFld_Cancelled = lMain.FindField("Cancelled");

            lFld_DDockey = lDtl.FindField("DOCKEY");
            lFld_DtlKey = lDtl.FindField("DTLKEY");
            lFld_Seq = lDtl.FindField("SEQ");
            lFld_ItemCode = lDtl.FindField("ItemCode");
            lFld_Qty = lDtl.FindField("QTY");
            lFld_UOM = lDtl.FindField("UOM");
            lFld_DDesc = lDtl.FindField("DESCRIPTION");
            lFld_UCost = lDtl.FindField("UnitCost");

            lDate = DateTime.Parse("January 1, 2020");

            BizObject.New();
            // Begin Append Master
            lFld_MDockey.Value = -1;
            lFld_DocNo.AsString = "--AJ Test--";
            lFld_DocDate.Value = lDate;
            lFld_PostDate.Value = lDate;
            lFld_Desc.AsString = "Stock Adjustment Description";
            lFld_Cancelled.AsString = "F";

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 1;
            lFld_ItemCode.AsString = "ANT";
            lFld_DDesc.AsString = "Adjust IN Item";
			lFld_UOM.AsString = "UNIT";
            lFld_Qty.AsFloat = 3;
            lFld_UCost.AsFloat = 25.15;//Only IN need UnitCost
            lDtl.Post();

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 2;
            lFld_ItemCode.AsString = "E-BAT";
            lFld_DDesc.AsString = "Adjust OUT Item";
			lFld_UOM.AsString = "UNIT";
            lFld_Qty.AsFloat = -4;
            lDtl.Post();

            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnSTXF_Click(object sender, EventArgs e)
        {
            dynamic lDate, lMain, lDtl, BizObject, lFld_MDockey, lFld_DocNo,
                    lFld_DocDate, lFld_PostDate, lFld_Desc,
                    lFld_Cancelled, lFld_DDockey, lFld_DtlKey,
                    lFld_Seq, lFld_ItemCode, lFld_Qty, lFld_UOM, lFld_DDesc, 
                    lFld_FromLoc, lFld_ToLoc;

            CheckLogin(0);

            BizObject = ComServer.BizObjects.Find("ST_XF");
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDtl = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data

            lFld_MDockey = lMain.FindField("DOCKEY");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Desc = lMain.FindField("Description");
            lFld_Cancelled = lMain.FindField("Cancelled");

            lFld_DDockey = lDtl.FindField("DOCKEY");
            lFld_DtlKey = lDtl.FindField("DTLKEY");
            lFld_Seq = lDtl.FindField("SEQ");
            lFld_ItemCode = lDtl.FindField("ItemCode");
            lFld_Qty = lDtl.FindField("QTY");
            lFld_UOM = lDtl.FindField("UOM");
            lFld_DDesc = lDtl.FindField("DESCRIPTION");
            lFld_FromLoc = lDtl.FindField("FROMLOCATION");
            lFld_ToLoc = lDtl.FindField("TOLOCATION");

            lDate = DateTime.Parse("January 1, 2020");

            BizObject.New();
            // Begin Append Master
            lFld_MDockey.Value = -1;
            lFld_DocNo.AsString = "--XF Test--";
            lFld_DocDate.Value = lDate;
            lFld_PostDate.Value = lDate;
            lFld_Desc.AsString = "Stock Transfer Description";
            lFld_Cancelled.AsString = "F";

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 1;
            lFld_ItemCode.AsString = "ANT";
            lFld_DDesc.AsString = "ANTENNA";
			lFld_UOM.AsString = "UNIT";
            lFld_Qty.AsFloat = 3;
            lFld_FromLoc.AsString = "----";
            lFld_ToLoc.AsString = "BALAKONG";
            lDtl.Post();

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 2;
            lFld_ItemCode.AsString = "E-BAT";
            lFld_DDesc.AsString = "ERICSSON BATTERY";
			lFld_UOM.AsString = "UNIT";
            lFld_Qty.AsFloat = 4;
            lFld_FromLoc.AsString = "KL";
            lFld_ToLoc.AsString = "BC";
            lDtl.Post();

            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnGLJE_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail;
            DateTime lDate;

            // Step 1: Create Com Server object
            CheckLogin();

            // Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("GL_JE");

            // Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data  

            lDate = DateTime.Parse("January 1, 2020");
            BizObject.New();
            lMain.FindField("DocKey").value = -1;
            lMain.FindField("DocNo").value = "--JV Test--";
            lMain.FindField("DocDate").value = lDate;
            lMain.FindField("PostDate").value = lDate;
            lMain.FindField("Description").value = "testing desc header";
            lMain.Post();


            // Step 7: Append Detail
            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("Code").value = "610-1000"; //Account GL Code
            lDetail.FindField("Description").value = "testing desc1";
            lDetail.FindField("Project").value = "P12W1";
            lDetail.FindField("Tax").value = "";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("LocalDR").value = 200;
            lDetail.FindField("DR").value = 200;
            lDetail.Post();

            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("Code").value = "605-200";
            lDetail.FindField("Description").value = "testing desc2";
            lDetail.FindField("Project").value = "P13W1";
            lDetail.FindField("Tax").value = "";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("LocalCR").value = 200;
            lDetail.FindField("CR").value = 200;
            lDetail.Post();

            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("Code").value = "610-000";
            lDetail.FindField("Description").value = "testing desc3";
            lDetail.FindField("Project").value = "P12W1";
            lDetail.FindField("Tax").value = "";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("LocalDR").value = 500.1;
            lDetail.FindField("DR").value = 500.1;
            lDetail.Post();

            lDetail.Append();
            lDetail.FindField("DtlKey").value = -1;
            lDetail.FindField("Code").value = "605-300";
            lDetail.FindField("Description").value = "testing desc4";
            lDetail.FindField("Project").value = "P13W1";
            lDetail.FindField("Tax").value = "";
            lDetail.FindField("TaxInclusive").value = 0;
            lDetail.FindField("LocalCR").value = 500.1;
            lDetail.FindField("CR").value = 500.1;
            lDetail.Post();

            // Step 8: Save Document
            BizObject.Save();
            BizObject.Close();

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Step 9 : Logout after done  
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnGLPV_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail;
            DateTime lDate;

            // Step 1: Create Com Server object
            CheckLogin();

            // Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("GL_PV");

            // Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data  

            lDate = DateTime.Parse("January 1, 2020");
            BizObject.New();
            lMain.FindField("DocKey").value = -1;
            lMain.FindField("DocNo").value = "--CB PV Test-";
            lMain.FindField("DocDate").Value = lDate;
            lMain.FindField("PostDate").Value = lDate;
            lMain.FindField("Description").AsString = "Compacc System";
            lMain.FindField("PaymentMethod").AsString = "310-001";
            lMain.FindField("CHEQUENUMBER").AsString = "MBB 213245";
            lMain.FindField("DocAmt").AsFloat = 2019.57;
            lMain.FindField("Cancelled").AsString = "F";
            lMain.Post();

            // Step 7: Append Detail
            lDetail.Append();
            lDetail.FindField("DTLKEY").value = -1;
            lDetail.FindField("DOCKEY").value = -1;
            lDetail.FindField("Code").AsString = "200-300";
            lDetail.FindField("DESCRIPTION").AsString = "Maybank - Asus A555LD-xx313H";
            lDetail.FindField("TAX").AsString = "";
            lDetail.FindField("TAXAMT").AsFloat = 0;
            lDetail.FindField("TAXINCLUSIVE").AsFloat = 0;
            lDetail.FindField("AMOUNT").AsFloat = 2019.57;
            lDetail.Post();

            // Step 8: Save Document
            BizObject.Save();
            BizObject.Close();

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Step 9 : Logout after done  
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnXFInfo1_Click(object sender, EventArgs e)
        {
            //This Example to get DO Number & Date that Tranfer to Invoice for given Invoice No.
            dynamic lxFer;
            string lSQL;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            lbTime.Text = "Get DO Information...";
            lSQL = "SELECT DOCNO, DOCDATE FROM SL_DO ";
            lSQL = lSQL + "WHERE DOCKEY = ( ";
            lSQL = lSQL + "SELECT FIRST 1 B.FROMDOCKEY FROM SL_IV A ";
            lSQL = lSQL + "INNER JOIN SL_IVDTL B ON(A.DOCKEY = B.DOCKEY) ";
            lSQL = lSQL + "WHERE FROMDOCTYPE = 'DO' ";
            lSQL = lSQL + "AND A.DOCNO = " + QuotedStr(edDocNo.Text);
            lSQL = lSQL + ")";
            lxFer = ComServer.DBManager.NewDataSet(lSQL);

            if (lxFer.RecordCount != 0)
            {
                edDesc.Text = lxFer.FindField("DocNo").AsString;
            }
            else
            {
                edDesc.Text = "No Record Found";
            }
            FreeBiz(lxFer);
            Logout();
            lbTime.Text = "";
        }

        private void BtnXFInfo2_Click(object sender, EventArgs e)
        {
            //This Example to get IV Number & Date that Tranfer From DO for given DO No.
            dynamic lxFer;
            string lSQL;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            lbTime.Text = "Get Invoice Information...";
            lSQL = "SELECT A.DOCNO, A.DOCDATE FROM SL_IV A ";
            lSQL = lSQL + "INNER JOIN SL_IVDTL B ON(A.DOCKEY = B.DOCKEY) ";
            lSQL = lSQL + "WHERE FROMDOCTYPE = 'DO' ";
            lSQL = lSQL + "AND FROMDOCKEY = (SELECT DOCKEY FROM SL_DO ";
            lSQL = lSQL + "WHERE DOCNO = " + QuotedStr(edDocNo.Text); 
            lSQL = lSQL + ")";
            lxFer = ComServer.DBManager.NewDataSet(lSQL);

            if (lxFer.RecordCount != 0)
            {
                edDesc.Text = lxFer.FindField("DocNo").AsString;
            }
            else
            {
                edDesc.Text = "No Record Found";
            }
            FreeBiz(lxFer);
            Logout();
            lbTime.Text = "";
        }

        private void btnARCT_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet, BizObject, lMain, lDetail;
            object[] V = new object[2];
            DateTime lDate;
            string lIVNO;

            CheckLogin(0);

            lSQL = "SELECT Dockey FROM AR_CT ";
            lSQL = lSQL + "WHERE DocNo='--CT Test--' ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_CT");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsKnockOff"); //lDetail contains detail data  

            //Step 4 : Posting
            lDate = DateTime.Parse("January 23, 2020");
            if (lDataSet.RecordCount > 0) //Found Do Edit
            {
                BizObject.Params.Find("DocKey").Value = lDataSet.FindField("DocKey").AsString;
                BizObject.Open();
                BizObject.Edit();
                lMain.Edit();
                lMain.FindField("Description").AsString = "Edited Description 123";

                //Step 5: Knock Off IV
                lIVNO = "IV-00021";
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 52; //Partial Knock off 
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }
            }
            else
            {
                BizObject.New();
                lMain.FindField("DOCKEY").Value = -1;
                lMain.FindField("DocNo").AsString = "--CT Test--";
                lMain.FindField("CODE").AsString = "300-A0002"; //Customer Account
                lMain.FindField("DocDate").Value = lDate;
                lMain.FindField("PostDate").Value = lDate;
                lMain.FindField("Description").AsString = "Contra";
                lMain.FindField("DocAmt").AsFloat = 200.00;
                lMain.FindField("Cancelled").AsString = "F";

                //Step 5: Knock Off IV
                lIVNO = "IV-00004";
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 147.09; //Partial Knock off
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }
            }


            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
            //Step 10 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(lDataSet);
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnLedgerRO_Click(object sender, EventArgs e)
        {
            dynamic RptObject, lDateFrom, lDateTo, lDataSet;

            // Step 1: Create Com Server object
            CheckLogin();
            // Step 2: Find and Create the Report Objects
            RptObject = ComServer.RptObjects.Find("GL.Ledger.RO");
            // Step 3: Spool parameters
            lDateFrom = DateTime.Parse("July 01, 2020");
            lDateTo = DateTime.Parse("July 31, 2020");

            RptObject.Params.Find("DateFrom").Value = lDateFrom;
            RptObject.Params.Find("UseDescription2").Value = false;
            RptObject.Params.Find("ShowLocal").Value = true;
            RptObject.Params.Find("ShowForeign").Value = false;
            RptObject.Params.Find("SelectDate").Value = true;
            RptObject.Params.Find("DateTo").Value = lDateTo;
            //RptObject.Params.Find("ProjectData").Value = //Not use if AllProject is true
            RptObject.Params.Find("MergeGLCode").Value = true;
            RptObject.Params.Find("AllProject").Value = true;
            RptObject.Params.Find("AllAccount").Value = false;
            RptObject.Params.Find("AccountType").Value = "G"; //G = General Ledger, P = Purchase Ledger, S = Sales Ledger
            RptObject.Params.Find("AccountData").Value = "310-001" + Environment.NewLine + "500-0000";
            //RptObject.Params.Find("ControlAccountData").Value = //Not use if AllControlAccount is true
            RptObject.Params.Find("AllControlAccount").Value = true;
            RptObject.Params.Find("AllAgent").Value = true;
            RptObject.Params.Find("AllArea").Value = true;
            RptObject.Params.Find("GroupBy").Value = "Code";
            RptObject.Params.Find("IncludeZeroClosingBalance").Value = true;
            RptObject.Params.Find("IncludeZeroTransaction").Value = true;
            // RptObject.Params.Find("AgentData").Value = //Not use if AllAgent is true
            //RptObject.Params.Find("TaxData").Value = //Not use if AllTax is true
            RptObject.Params.Find("AllTax").Value = true;
            //RptObject.Params.Find("AreaData=").Value = //Not use if AllArea is true
            RptObject.Params.Find("ExcludeProjectWhenMerge").Value = false;
            RptObject.Params.Find("ByTaxDate").Value = false;
            RptObject.Params.Find("GroupControlAccount").Value = false;
            RptObject.Params.Find("SortBy").Value = "Code;AccDesc;PostDate";

            //Step 4: Perform Report calculation 
            RptObject.CalculateReport();

            lDataSet = RptObject.DataSets.Find("cdsMain");

            // Step 5 : Convert to Dataset
            Tbl.Tables.Clear();
            PrepareData(lDataSet, "Master");

            // Step 6 : Bind the Master to the dataset.
            MasterBS.DataSource = Tbl;
            MasterBS.DataMember = "Master";

            // Step 7 : Set Grid Info
            DataGVM.DataSource = MasterBS;
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            FreeBiz(lDataSet);
            //Logout();
            RptObject = null;
        }

        private void BtnARIV_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail, 
                    lFld_MDockey, lFld_DocNo,
                    lFld_Code, lFld_DocDate, lFld_PostDate, lFld_Desc,
                    lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_DDesc,
                    lFld_Tax, lFld_TaxRate, lFld_TaxInc,
                    lFld_Amt, lFld_TaxAmt;

            DateTime lDate;

            CheckLogin(0);
            lbTime.Text = "Posting Customer Invoice...";
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_IV");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  

            lFld_MDockey = lMain.FindField("DocKey");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Code = lMain.FindField("Code");
            lFld_Desc = lMain.FindField("Description");

            lFld_DDockey = lDetail.FindField("DocKey");
            lFld_DtlKey = lDetail.FindField("DtlKey");
            lFld_Seq = lDetail.FindField("Seq");
            lFld_DDesc = lDetail.FindField("Description");
            lFld_Tax = lDetail.FindField("Tax");
            lFld_TaxRate = lDetail.FindField("TaxRate");
            lFld_TaxInc = lDetail.FindField("TaxInclusive");
            lFld_Amt = lDetail.FindField("Amount");
            lFld_TaxAmt = lDetail.FindField("TaxAmt");

            //Step 4 : Insert Data - Master
            lDate = DateTime.Parse("January 11, 2021");
            BizObject.New();
            lFld_MDockey.value = -1;
            lFld_DocNo.AsString = "--IV Test--";
            lFld_DocDate.value = lDate;
            lFld_PostDate.value = lDate;
            lFld_Code.AsString = "300-C0001"; //Customer Account
            lFld_Desc.AsString = "Invoice";

            //Step 5: Insert Data - Detail
            lDetail.Append();
            lFld_DtlKey.value = -1;
            lFld_DDockey.value = -1;
            lFld_Seq.value = 1;
            lFld_DDesc.AsString = "Sales Item A";
            lDetail.FindField("Account").AsString     = "500-000";
            lFld_Tax.AsString = "SV";
            lFld_TaxRate.AsString = "6%";
            lFld_TaxInc.value = 0;
            lFld_Amt.AsFloat = 200;
            lFld_TaxAmt.AsFloat = 12;
            lDetail.Post();

            lDetail.Append();
            lFld_DtlKey.value = -1;
            lFld_DDockey.value = -1;
            lFld_Seq.value = 2;
            lFld_DDesc.AsString = "Sales Item B";
            lDetail.FindField("Account").AsString = "500-3000";
            lFld_Tax.AsString = "SV";
            lFld_TaxRate.AsString = "6%";
            lFld_TaxInc.value = 0;
            lFld_Amt.AsFloat = 30;
            lFld_TaxAmt.AsFloat = 1.80;
            lDetail.Post();

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();
        }

        private void BtnARPMBounce_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDocKey;
            DateTime lDate;
            // Step 1: Create Com Server object
            CheckLogin();

            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_PM");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet"); //lMain contains master data

            //Step 4 : Find CN Number
            //lDocNo = "-- Test--";
            lDocKey = BizObject.FindKeyByRef("DocNo", edDocNo.Text);
            BizObject.Params.Find("DocKey").Value = lDocKey;

            if (!Convert.IsDBNull(lDocKey))
            {
                lDate = DateTime.Parse("January 11, 2021");
                BizObject.Open();
                BizObject.Edit();
                lMain.Edit();
                lMain.FindField("BOUNCEDDATE").value = lDate;
                //Step 5: Save Document
                BizObject.Save();
                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Record Not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            BizObject.Close();
            FreeBiz(BizObject);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail, lKO,
                    lFld_MDockey, lFld_DocNo,
                    lFld_Code, lFld_DocDate, lFld_PostDate, lFld_Desc,
                    lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_DDesc,
                    lFld_Tax, lFld_TaxRate, lFld_TaxInc,
                    lFld_Amt, lFld_TaxAmt;
            object[] V = new object[2];
            DateTime lDate;
            string lIVNO;

            CheckLogin(0);
            lbTime.Text = "Posting Customer Credit Note...";
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_CN");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  
            lKO = BizObject.DataSets.Find("cdsKnockOff");

            lFld_MDockey = lMain.FindField("DocKey");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Code = lMain.FindField("Code");
            lFld_Desc = lMain.FindField("Description");

            lFld_DDockey = lDetail.FindField("DocKey");
            lFld_DtlKey = lDetail.FindField("DtlKey");
            lFld_Seq = lDetail.FindField("Seq");
            lFld_DDesc = lDetail.FindField("Description");
            lFld_Tax = lDetail.FindField("Tax");
            lFld_TaxRate = lDetail.FindField("TaxRate");
            lFld_TaxInc = lDetail.FindField("TaxInclusive");
            lFld_Amt = lDetail.FindField("Amount");
            lFld_TaxAmt = lDetail.FindField("TaxAmt");

            //Step 4 : Insert Data - Master
            lDate = DateTime.Parse("January 11, 2021");
            BizObject.New();
            lFld_MDockey.value = -1;
            lFld_DocNo.AsString = "--CN Test--";
            lFld_DocDate.value = lDate;
            lFld_PostDate.value = lDate;
            lFld_Code.AsString = "300-C0001"; //Customer Account
            lFld_Desc.AsString = "Credit Note";

            //Step 5: Insert Data - Detail
            lDetail.Append();
            lFld_DtlKey.value = -1;
            lFld_DDockey.value = -1;
            lFld_Seq.value = 1;
            lFld_DDesc.AsString = "Sales Item A";
            lDetail.FindField("Account").AsString = "500-000";
            lFld_Tax.AsString = "SV";
            lFld_TaxRate.AsString = "6%";
            lFld_TaxInc.value = 0;
            lFld_Amt.AsFloat = 200;
            lFld_TaxAmt.AsFloat = 12;
            lDetail.Post();

            lDetail.Append();
            lFld_DtlKey.value = -1;
            lFld_DDockey.value = -1;
            lFld_Seq.value = 2;
            lFld_DDesc.AsString = "Sales Item B";
            lDetail.FindField("Account").AsString = "500-3000";
            lFld_Tax.AsString = "SV";
            lFld_TaxRate.AsString = "6%";
            lFld_TaxInc.value = 0;
            lFld_Amt.AsFloat = 10;
            lFld_TaxAmt.AsFloat = 0.60;
            lDetail.Post();

            //Step 6: Knock Off IV
            lIVNO = "--IV Test--";
            V[0] = "IV";
            V[1] = lIVNO;

            if (lKO.Locate("DocType;DocNo", V, false, false))
            {
                lKO.Edit();
                lKO.FindField("KOAmt").AsFloat = 147.09; //Partial Knock off
                lKO.FindField("KnockOff").AsString = "T";
                lKO.Post();
            }

            //Step 7: Save Document
            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();

        }

        private void BtnAddSTLoc_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDocKey;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("ST_Location");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data

            //Step 4: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);

            //Step 5 : Insert or Update
            if (Convert.IsDBNull(lDocKey))
            {
                BizObject.New();
                lMain.FindField("CODE").value = edCode.Text;
                lMain.FindField("Description").value = edDesc.Text;
                lMain.FindField("Address1").AsString = "Address1";
                lMain.FindField("Address2").AsString = "Address2";
                lMain.FindField("Address3").AsString = "Address3";
                lMain.FindField("Address4").AsString = "Address4";
                lMain.FindField("Attention").AsString = "Attention";
                lMain.FindField("Phone1").AsString = "Phone1";
                lMain.FindField("Fax1").AsString = "Fax1";
                lMain.FindField("Email").AsString = "EmailAddress";

            }
            else
            {//Edit Data if found
                BizObject.Params.Find("Code").Value = lDocKey;
                BizObject.Open();
                BizObject.Edit();
                lMain.FindField("Description").value = edDesc.Text;
                lMain.FindField("Address1").AsString = "Address1-Changed";
                lMain.FindField("Address2").AsString = "Address2-Changed";
                lMain.FindField("Address3").AsString = "Address3-Changed";
                lMain.FindField("Address4").AsString = "Address4-Changed";
                lMain.FindField("Attention").AsString = "Attention-Changed";
                lMain.FindField("Phone1").AsString = "Phone1-Changed";
                lMain.FindField("Fax1").AsString = "Fax1-Changed";
                lMain.FindField("Email").AsString = "EmailAddress-Changed";
            }
            //Step 6: Save & Close
            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Step 7 : Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail, lxFer;

            string lSQL;
            DateTime lDate;
            
            CheckLogin(0);
            Logout();
            // Check Is Transferred or not
            lbTime.Text = "Check Transfer Status...";
            lSQL = "SELECT Dockey, DocNo,Code, CompanyName, NSeq, Seq, DtlKey, ItemCode, Description, ";
            lSQL = lSQL + "Qty, UOM, UnitPrice, Disc, Tax, TaxRate, TaxInclusive, TaxAmt, Amount, ";
            lSQL = lSQL + "COALESCE(Sum(XFQty),0) XFQty, COALESCE((Qty-Sum(XFQty)), Qty) OSQty FROM ( ";
            lSQL = lSQL + "SELECT * FROM ( ";
            lSQL = lSQL + "WITH DOC_SEQ AS (SELECT row_number() OVER (PARTITION BY Dockey ORDER BY Dockey, Seq) AS SEQ, ";
            lSQL = lSQL + "                 DTLKEY FROM SL_SODTL) ";
            lSQL = lSQL + "SELECT A.Dockey, A.DocNo, A.Code, A.CompanyName, ";
            lSQL = lSQL + "B.DtlKey, D.Seq NSeq, B.Seq, B.ItemCode, B.Description, B.Qty, B.UOM, ";
            lSQL = lSQL + "B.UnitPrice, B.Disc, B.Amount, B.Tax, B.TaxRate, B.TaxInclusive, B.TaxAmt,  ";
            lSQL = lSQL + "C.Qty XFQty FROM SL_SO A  ";
            lSQL = lSQL + "INNER JOIN SL_SODTL B ON (A.DOCKEY=B.DOCKEY)  ";
            lSQL = lSQL + "INNER JOIN DOC_SEQ D ON (B.DTLKEY=D.DTLKEY)  ";
            lSQL = lSQL + "LEFT JOIN ST_XTRANS C ON (A.DOCKEY=C.FROMDOCKEY AND B.DTLKEY=C.FROMDTLKEY  ";
            lSQL = lSQL + "                          AND C.FROMDOCTYPE='SO')  ";
            lSQL = lSQL + "WHERE A.DOCNO='SO-00002')  ";
            lSQL = lSQL + ")  ";
            lSQL = lSQL + "GROUP BY Dockey, DocNo, Code, CompanyName, NSeq, Seq, Dtlkey, ItemCode, Description,  ";
            lSQL = lSQL + "Qty, UOM, UnitPrice, Disc, Tax, TaxRate, TaxInclusive, TaxAmt, Amount  ";
            lSQL = lSQL + "HAVING COALESCE((Qty-Sum(XFQty)), Qty) >0  ";
            lxFer = ComServer.DBManager.NewDataSet(lSQL);

            if (lxFer.RecordCount > 0)
            {
                lbTime.Text = "Posting DO...";
                //Find and Create the Biz Objects
                BizObject = ComServer.BizObjects.Find("SL_DO");

                //Set Dataset
                lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
                lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  

                lbTime.Text = "Transfer SO to DO...";
                lDate = DateTime.Parse("December 27, 2022");
                BizObject.New();
                lMain.FindField("DocNo").AsString = "--DO Test--";
                lMain.FindField("DocDate").value = lDate;
                lMain.FindField("PostDate").value = lDate;
                lMain.FindField("Code").AsString = "300-C0001"; //Customer Account
                lMain.FindField("CompanyName").AsString = "Customer A";
                //lMain.FindField("Address1").AsString = ""; //Optional
                //lMain.FindField("Address2").AsString = ""; //Optional
                //lMain.FindField("Address3").AsString = ""; //Optional
                //lMain.FindField("Address4").AsString = ""; //Optional
                //lMain.FindField("Phone1").AsString = "";   //Optional
                lMain.FindField("Description").AsString = "Delivery Order";

                lxFer.First();
                while ((!lxFer.Eof))
                {
                    lDetail.Append();
                    lDetail.FindField("Seq").value = lxFer.FindField("Seq").Value;
                    lDetail.FindField("ItemCode").AsString = lxFer.FindField("ItemCode").AsString;
                    lDetail.FindField("Description").AsString = lxFer.FindField("Description").AsString;
                    lDetail.FindField("UOM").AsString = lxFer.FindField("UOM").AsString;
                    lDetail.FindField("Qty").AsFloat = lxFer.FindField("OSQty").AsFloat;
                    lDetail.FindField("Tax").AsString = lxFer.FindField("Tax").AsString;
                    lDetail.FindField("TaxRate").AsString = lxFer.FindField("TaxRate").AsString;
                    lDetail.FindField("TaxInclusive").value = lxFer.FindField("TaxInclusive").Value;
                    lDetail.FindField("UnitPrice").AsFloat = lxFer.FindField("UnitPrice").AsFloat;
                    //lDetail.FindField("Amount").AsFloat = //System Auto Calc;
                    //lDetail.FindField("TaxAmt").AsFloat = //System Auto Calc;
                    lDetail.FindField("FromDocType").AsString = "SO";
                    lDetail.FindField("FromDockey").Value = lxFer.FindField("DocKey").Value;
                    lDetail.FindField("FromDtlKey").Value = lxFer.FindField("DtlKey").Value;
                    lDetail.Post();
                    lxFer.Next();
                }
                BizObject.Save();
                BizObject.Close();
                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FreeBiz(BizObject);
            }
            else
            {
                MessageBox.Show("Record not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            FreeBiz(lxFer);
            Logout();
            lbTime.Text = "";
        }

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            string lSQL;
            dynamic lDataSet;

            // Create Com Server object
            CheckLogin();
            //Thread.Sleep(5000); //will sleep for 5 sec
            // Get Option
            lSQL = "SELECT RNAME, RVALUE FROM SY_REGISTRY ";
            lSQL = lSQL + "WHERE RNAME IN ('CustomerOneCentDiffRounding', ";
            lSQL = lSQL + "'CustomerRTN5CentsCS', 'CustomerRTN5CentsIV') ";

            try
            {
                lDataSet = ComServer.DBManager.NewDataSet(lSQL);

                Tbl.Tables.Clear();
                PrepareData(lDataSet, "Master");

                // Bind the Master to the dataset.
                MasterBS.DataSource = Tbl;
                MasterBS.DataMember = "Master";

                // Set Datasoure to Grid
                DataGVM.DataSource = MasterBS;
                FreeBiz(lDataSet);
            }
            catch (Exception ex)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(ex.Message, "^Error writing data to the connection\\."))
                {
                    ComServer.Reconnect();
                    lDataSet = ComServer.DBManager.NewDataSet(lSQL);
                    Tbl.Tables.Clear();
                    PrepareData(lDataSet, "Master");
                    // Bind the Master to the dataset.
                    MasterBS.DataSource = Tbl;
                    MasterBS.DataMember = "Master";

                    // Set Datasoure to Grid
                    DataGVM.DataSource = MasterBS;
                    FreeBiz(lDataSet);
                }

            }

            Logout();
        }

        private void btnSTBatch_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDtl, lDup;
            string lSQL, lCode;

            CheckLogin(0);
            lbTime.Text = "Check Duplicate Status...";
            lSQL = "SELECT Code FROM ST_BATCH ";
            lSQL = lSQL + "WHERE Code=" + QuotedStr(edCode.Text);
            lDup = ComServer.DBManager.NewDataSet(lSQL);

            if (lDup.RecordCount > 0)
            {
                lCode = lDup.FindField("Code").AsString;
            }
            else
            {
                lCode = "";
            }
            BizObject = ComServer.BizObjects.Find("ST.BATCH.OPF");

            //Set Dataset
            lMain = BizObject.DataSets.Find("Main");  //lMain contains master data
            lDtl = BizObject.DataSets.Find("BatchItem");        //lDtl contains UOM data

            //Insert or Update
            if (lCode == "")
            {
                BizObject.New();
                lMain.FindField("CODE").AsString = edCode.Text;
                lMain.FindField("Description").AsString = edDesc.Text;
                lMain.FindField("OISACTIVE").value = 1;

                lDtl.Append(); //For 1St ItemCode
                lDtl.FindField("ItemCode").AsString = "BOM";
                lDtl.Post();

                lDtl.Append(); //For 2nd ItemCode
                lDtl.FindField("ItemCode").AsString = "E-BAT";
                lDtl.Post();
            }
            else
            {//Edit Data if found
                BizObject.Params.Find("Code").AsString = edCode.Text;
                BizObject.Open();
                BizObject.Edit();
                lMain.FindField("Description").AsString = edDesc.Text;
                lMain.FindField("OISACTIVE").value = 0;

                // Delete all Detail
                while (lDtl.RecordCount != 0)
                {
                    lDtl.First();
                    lDtl.Delete();
                }

                lDtl.Append(); //For 1St ItemCode
                lDtl.FindField("ItemCode").AsString = "ANT";
                lDtl.Post();

            }
            //Save & Close
            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void btnStatement_Click(object sender, EventArgs e)
        {
            // C1 - Current Month
            // C2 - 1 Month
            // C3 - 2 Months 
            // C4 - 3 Months
            // C5 - 4 Months 
            // C6 - 5 Months & above

            const string Quote = "\"";
            dynamic RptObject, lDataSet1, lDataSet2, lDateTo, lDateFrom, lAgeData;
            DataColumn tblC1, tblC2;
            DataRelation drt;

            // Step 1: Create Com Server object
            CheckLogin();
            // Step 2: Find and Create the Report Objects
            RptObject = ComServer.RptObjects.Find("Customer.Statement.RO");
            // Step 3: Spool parameters
            //6 Months Aging
            //lAgeData = "<?xml version=" + Quote + "1.0" + Quote + " standalone=" + Quote + "yes" + Quote + "?>  <DATAPACKET Version=" + Quote + "2.0" + Quote + "><METADATA><FIELDS>" + "<FIELD attrname=" + Quote + "ColumnNo" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/><FIELD attrname=" + Quote + "ColumnType" + Quote + " fieldtype=" + Quote + "string" + Quote + " WIDTH=" + Quote + "1" + Quote + "/>" + "<FIELD attrname=" + Quote + "Param1" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/><FIELD attrname=" + Quote + "Param2" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/>" + "<FIELD attrname=" + Quote + "IsLocal" + Quote + " fieldtype=" + Quote + "boolean" + Quote + "/><FIELD attrname=" + Quote + "HeaderScript" + Quote + " fieldtype=" + Quote + "bin.hex" + Quote + " SUBTYPE=" + Quote + "Text" + Quote + " WIDTH=" + Quote + "1" + Quote + "/>" + "</FIELDS><PARAMS/></METADATA><ROWDATA><ROW ColumnNo=" + Quote + "0" + Quote + " ColumnType=" + Quote + "" + Quote + " Param1=" + Quote + "0" + Quote + " Param2=" + Quote + "0" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "1" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "0" + Quote + " Param2=" + Quote + "0" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;Current Mth&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "2" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-1" + Quote + " Param2=" + Quote + "-1" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;1 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "3" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-2" + Quote + " Param2=" + Quote + "-2" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;2 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "4" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-3" + Quote + " Param2=" + Quote + "-3" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;3 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "5" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-4" + Quote + " Param2=" + Quote + "-4" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;4 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "6" + Quote + " ColumnType=" + Quote + "B" + Quote + " Param1=" + Quote + "-999999" + Quote + " Param2=" + Quote + "-5" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;5 Month &amp; above&apos;&#013;end;" + Quote + "/>" + "</ROWDATA></DATAPACKET>";
            //lAgeData = richTextBox1.Text; //for 12 Months Aging
            lAgeData = "<?xml version=" + Quote + "1.0" + Quote + " standalone=" + Quote + "yes" + Quote + "?>  <DATAPACKET Version=" + Quote + "2.0" + Quote + "><METADATA><FIELDS>" +
                       "<FIELD attrname=" + Quote + "ColumnNo" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/><FIELD attrname=" + Quote + "ColumnType" + Quote + " fieldtype=" + Quote + "string" + Quote + " WIDTH=" + Quote + "1" + Quote + "/>" +
                       "<FIELD attrname=" + Quote + "Param1" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/><FIELD attrname=" + Quote + "Param2" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/>" +
                       "<FIELD attrname=" + Quote + "IsLocal" + Quote + " fieldtype=" + Quote + "boolean" + Quote + "/><FIELD attrname=" + Quote + "HeaderScript" + Quote + " fieldtype=" + Quote + "bin.hex" + Quote + " SUBTYPE=" + Quote + "Text" + Quote + " WIDTH=" + Quote + "1" + Quote + "/>" +
                       "</FIELDS><PARAMS/></METADATA><ROWDATA><ROW ColumnNo=" + Quote + "0" + Quote + " ColumnType=" + Quote + "" + Quote + " Param1=" + Quote + "0" + Quote + " Param2=" + Quote + "0" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + "/>" +
                       "<ROW ColumnNo=" + Quote + "1" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "0" + Quote + " Param2=" + Quote + "0" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= +apos;Current Mth+apos;&#013;end;" + Quote + "/>" +
                       "<ROW ColumnNo=" + Quote + "2" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-1" + Quote + " Param2=" + Quote + "-1" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= +apos;1 Months+apos;&#013;end;" + Quote + "/>" +
                       "<ROW ColumnNo=" + Quote + "3" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-2" + Quote + " Param2=" + Quote + "-2" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= +apos;2 Months+apos;&#013;end;" + Quote + "/>" +
                       "<ROW ColumnNo=" + Quote + "4" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-3" + Quote + " Param2=" + Quote + "-3" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= +apos;3 Months+apos;&#013;end;" + Quote + "/>" +
                       "<ROW ColumnNo=" + Quote + "5" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-4" + Quote + " Param2=" + Quote + "-4" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= +apos;4 Months+apos;&#013;end;" + Quote + "/>" +
                       "<ROW ColumnNo=" + Quote + "6" + Quote + " ColumnType=" + Quote + "B" + Quote + " Param1=" + Quote + "-999999" + Quote + " Param2=" + Quote + "-5" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= +apos;5 Month +amp; above+apos;&#013;end;" + Quote + "/>" +
                       "</ROWDATA></DATAPACKET>";


            // RptObject.Params.Find("AgentData").Value            =  'Not use if AllAgent is True
            RptObject.Params.Find("AgingData").Value = lAgeData; // Fixed
            RptObject.Params.Find("AgingOn").Value = "I"; // Fixed
            RptObject.Params.Find("AllAgent").Value = true;
            RptObject.Params.Find("AllArea").Value = true;
            RptObject.Params.Find("AllCompany").Value = true;
            RptObject.Params.Find("AllCompanyCategory").Value = true;
            RptObject.Params.Find("AllControlAccount").Value = true;
            RptObject.Params.Find("AllCurrency").Value = true;
            RptObject.Params.Find("AllDocProject").Value = true;
            // RptObject.Params.Find("AreaData").Value             =  'Not use if AllArea is True
            // RptObject.Params.Find("CompanyCategoryData").Value  =  'Not use if AllCompanyCategory is True
            // RptObject.Params.Find("CompanyData").Value = "300-A0001" + Environment.NewLine + "300-C0001" //Filter by Customer Code 300-A0001 & 300-C0001
            // RptObject.Params.Find("ControlAccountData").Value   =  'Not use if AllControlAccount is True
            // RptObject.Params.Find("CurrencyData").Value         =  'Not use if AllCurrency is True
            // RptObject.Params.Find("DocProjectData").Value       =  'Not use if AllDocProject is True

            lDateFrom = DateTime.Parse("December 1, 2021");
            lDateTo = DateTime.Parse("December 29, 2021");
            RptObject.Params.Find("DateFrom").Value = lDateFrom;
            RptObject.Params.Find("DateTo").Value = lDateTo;

            
            RptObject.Params.Find("IncludeZeroBalance").Value = false;
            RptObject.Params.Find("SelectDate").Value = true;
            RptObject.Params.Find("SortBy").Value = "CompanyCategory;Code;CompanyName;Agent;Area;CurrencyCode;ControlAccount";
            RptObject.Params.Find("StatementDate").Value = lDateTo;
            RptObject.Params.Find("StatementType").Value = "O"; //O = Open Item, B = B/F

            // Step 4: Perform Report calculation 
            RptObject.CalculateReport();
            lDataSet1 = RptObject.DataSets.Find("cdsMain");
            lDataSet2 = RptObject.DataSets.Find("cdsAging");
            //MessageBox.Show("Count - Main " + lDataSet1.RecordCount, "Count", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //MessageBox.Show("Count - Document " + lDataSet2.RecordCount, "Count", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 5 : Convert to Dataset
            ClearTables();

            PrepareData(lDataSet1, "Master");
            PrepareData(lDataSet2, "Detail");

            // Step 6 : Get Relation column
             tblC1 = Tbl.Tables["Master"].Columns["Code"];
             tblC2 = Tbl.Tables["Detail"].Columns["Code"];
             drt = new DataRelation("relation", tblC1, tblC2);


            // Step 7 : Set Grid Info
            SetGridInfo(drt);

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            Logout();
            RptObject = null;
        }

        private void btnARPM_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet, BizObject, lMain, lDetail;
            object[] V = new object[2];
            DateTime lDate;
            string lIVNO;

            CheckLogin(0);

            lSQL = "SELECT Dockey FROM AR_PM ";
            lSQL = lSQL + "WHERE DocNo='--PM Test--' ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            //Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("AR_PM");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");   //lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsKnockOff"); //lDetail contains detail data  

            //Step 4 : Posting
            lDate = DateTime.Parse("January 23, 2023");
            if (lDataSet.RecordCount > 0) //Found Do Edit
            {
                BizObject.Params.Find("DocKey").Value = lDataSet.FindField("DocKey").AsString;
                BizObject.Open();
                BizObject.Edit();
                lMain.Edit();
                lMain.FindField("Description").AsString = "Edited Description 123";

                //Step 5: Knock Off IV
                lIVNO = "IV-00083"; //Edit change KO Amt
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 52; //Partial Knock off 
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }

                lIVNO = "IV-00100"; //Knock off 2nd IV
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 120.10; //Partial Knock off 
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }
            }
            else
            {
                BizObject.New();
                lMain.FindField("DOCKEY").Value = -1;
                lMain.FindField("DocNo").AsString = "--PM Test--";
                lMain.FindField("CODE").AsString = "300-C0001"; //Customer Account
                lMain.FindField("DocDate").Value = lDate;
                lMain.FindField("PostDate").Value = lDate;
                lMain.FindField("Description").AsString = "Payment for A/c";
                lMain.FindField("PaymentMethod").AsString = "320-000"; //Bank or Cash Account
                lMain.FindField("ChequeNumber").AsString = "";
                lMain.FindField("BankCharge").AsFloat = 0;
                lMain.FindField("DocAmt").AsFloat = 200.00;
                lMain.FindField("Cancelled").AsString = "F";
                lMain.Post();

                //Step 5: Knock Off IV
                lIVNO = "IV-00083";
                V[0] = "IV";
                V[1] = lIVNO;

                if (lDetail.Locate("DocType;DocNo", V, false, false))
                {
                    lDetail.Edit();
                    lDetail.FindField("KOAmt").AsFloat = 147.09; //Partial Knock off
                    lDetail.FindField("KnockOff").AsString = "T";
                    lDetail.Post();
                }
            }

            //Step 6: Save Document
            BizObject.Save();
            BizObject.Close();
            //Step 10 : Logout after done 
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(lDataSet);
            FreeBiz(BizObject);
            Logout();
        }

        private void btnSO2DO_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDetail, lxFer;

            string lSQL;
            DateTime lDate;

            CheckLogin(0);
            Logout();
            // Check Is Transferred or not
            lbTime.Text = "Check Transfer Status...";
            lSQL = "SELECT Dockey, DocNo,Code, CompanyName, NSeq, Seq, DtlKey, ItemCode, Description, ";
            lSQL = lSQL + "Qty, UOM, UnitPrice, Disc, Tax, TaxRate, TaxInclusive, TaxAmt, Amount, ";
            lSQL = lSQL + "COALESCE(Sum(XFQty),0) XFQty, COALESCE((Qty-Sum(XFQty)), Qty) OSQty FROM ( ";
            lSQL = lSQL + "SELECT * FROM ( ";
            lSQL = lSQL + "WITH DOC_SEQ AS (SELECT row_number() OVER (PARTITION BY Dockey ORDER BY Dockey, Seq) AS SEQ, ";
            lSQL = lSQL + "                 DTLKEY FROM SL_SODTL) ";
            lSQL = lSQL + "SELECT A.Dockey, A.DocNo, A.Code, A.CompanyName, ";
            lSQL = lSQL + "B.DtlKey, D.Seq NSeq, B.Seq, B.ItemCode, B.Description, B.Qty, B.UOM, ";
            lSQL = lSQL + "B.UnitPrice, B.Disc, B.Amount, B.Tax, B.TaxRate, B.TaxInclusive, B.TaxAmt,  ";
            lSQL = lSQL + "C.Qty XFQty FROM SL_SO A  ";
            lSQL = lSQL + "INNER JOIN SL_SODTL B ON (A.DOCKEY=B.DOCKEY)  ";
            lSQL = lSQL + "INNER JOIN DOC_SEQ D ON (B.DTLKEY=D.DTLKEY)  ";
            lSQL = lSQL + "LEFT JOIN ST_XTRANS C ON (A.DOCKEY=C.FROMDOCKEY AND B.DTLKEY=C.FROMDTLKEY  ";
            lSQL = lSQL + "                          AND C.FROMDOCTYPE='SO')  ";
            lSQL = lSQL + "WHERE A.DOCNO='SO-00002')  ";
            lSQL = lSQL + ")  ";
            lSQL = lSQL + "GROUP BY Dockey, DocNo, Code, CompanyName, NSeq, Seq, Dtlkey, ItemCode, Description,  ";
            lSQL = lSQL + "Qty, UOM, UnitPrice, Disc, Tax, TaxRate, TaxInclusive, TaxAmt, Amount  ";
            lSQL = lSQL + "HAVING COALESCE((Qty-Sum(XFQty)), Qty) >0  ";
            lxFer = ComServer.DBManager.NewDataSet(lSQL);

            if (lxFer.RecordCount > 0)
            {
                lbTime.Text = "Posting DO...";
                //Find and Create the Biz Objects
                BizObject = ComServer.BizObjects.Find("SL_DO");

                //Set Dataset
                lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data
                lDetail = BizObject.DataSets.Find("cdsDocDetail"); //lDetail contains detail data  

                lbTime.Text = "Transfer SO to DO...";
                lDate = DateTime.Parse("December 27, 2022");
                BizObject.New();
                lMain.FindField("DocNo").AsString = "--DO Test--";
                lMain.FindField("DocDate").value = lDate;
                lMain.FindField("PostDate").value = lDate;
                lMain.FindField("Code").AsString = "300-C0001"; //Customer Account
                lMain.FindField("CompanyName").AsString = "Customer A";
                //lMain.FindField("Address1").AsString = ""; //Optional
                //lMain.FindField("Address2").AsString = ""; //Optional
                //lMain.FindField("Address3").AsString = ""; //Optional
                //lMain.FindField("Address4").AsString = ""; //Optional
                //lMain.FindField("Phone1").AsString = "";   //Optional
                lMain.FindField("Description").AsString = "Delivery Order";

                lxFer.First();
                while ((!lxFer.Eof))
                {
                    lDetail.Append();
                    lDetail.FindField("Seq").value = lxFer.FindField("Seq").Value;
                    lDetail.FindField("ItemCode").AsString = lxFer.FindField("ItemCode").AsString;
                    lDetail.FindField("Description").AsString = lxFer.FindField("Description").AsString;
                    lDetail.FindField("UOM").AsString = lxFer.FindField("UOM").AsString;
                    lDetail.FindField("Qty").AsFloat = lxFer.FindField("OSQty").AsFloat;
                    lDetail.FindField("Tax").AsString = lxFer.FindField("Tax").AsString;
                    lDetail.FindField("TaxRate").AsString = lxFer.FindField("TaxRate").AsString;
                    lDetail.FindField("TaxInclusive").value = lxFer.FindField("TaxInclusive").Value;
                    lDetail.FindField("UnitPrice").AsFloat = lxFer.FindField("UnitPrice").AsFloat;
                    //lDetail.FindField("Amount").AsFloat = //System Auto Calc;
                    //lDetail.FindField("TaxAmt").AsFloat = //System Auto Calc;
                    lDetail.FindField("FromDocType").AsString = "SO";
                    lDetail.FindField("FromDockey").Value = lxFer.FindField("DocKey").Value;
                    lDetail.FindField("FromDtlKey").Value = lxFer.FindField("DtlKey").Value;
                    lDetail.Post();
                    lxFer.Next();
                }
                BizObject.Save();
                BizObject.Close();
                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FreeBiz(BizObject);
            }
            else
            {
                MessageBox.Show("Record not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            FreeBiz(lxFer);
            Logout();
            lbTime.Text = "";
        }

        private void btnLogInOut_Click(object sender, EventArgs e)
        {
            int I;
            // edLogout.Checked = true;
            for (I = 1; (I <= edRecord.Value); I++)
            {
                string lSQL;
                dynamic lDataSet;

                if (IsOdd(I))
                {
                    CheckLogin(0);
                }
                else
                {
                    CheckLogin(1);
                }
                lSQL = "SELECT DESCRIPTION FROM AGENT ";
                lSQL = lSQL + "ORDER BY RAND()";

                lDataSet = ComServer.DBManager.NewDataSet(lSQL);
                edDesc.Text = lDataSet.FindField("Description").AsString;
                lbTime.Text = "Record No : " + I;

                FreeBiz(lDataSet);
                ComServer.Logout();
                FreeBiz(ComServer);
                Thread.Sleep(5000); //Sleep for 5 seconds.
                
            }
        }

        private void btnDCFDBList_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog()
            {
                Title = "Open DCF File...",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "DCF File (*.DCF)|*.DCF|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (fd.ShowDialog() == DialogResult.OK)
            {
                string[] DCFFile = System.IO.File.ReadAllLines(fd.FileName);
                List<string> SL = new List<string>();
                string Lnstr, RStr;
                string[] strArr, RStrArr;
                DataColumn col;
                DataRow row;
                int I;

                foreach (string line in DCFFile)
                    SL.Add(line.Trim());

                Tbl.Tables.Add("DCFList");
                // Add Columns
                col = new DataColumn("Database");
                Tbl.Tables["DCFList"].Columns.Add(col);
                col = new DataColumn("CompanyName");
                Tbl.Tables["DCFList"].Columns.Add(col);
                col = new DataColumn("Remark");
                Tbl.Tables["DCFList"].Columns.Add(col);
                col = new DataColumn("Version");
                Tbl.Tables["DCFList"].Columns.Add(col);

                for (I = 2; I <= SL.Count - 2; I++)
                {
                    row = Tbl.Tables["DCFList"].Rows.Add();
                    Lnstr = SL[I]; // Get Row Index No 2
                    strArr = Lnstr.Split('=');
                    // Database
                    RStr = strArr[1];
                    RStrArr = RStr.Split('\"');
                    row[0] = RStrArr[1];

                    // CompanyName
                    RStr = strArr[2];
                    RStrArr = RStr.Split('\"');
                    row[1] = RStrArr[1];

                    // Remark
                    RStr = strArr[3];
                    RStrArr = RStr.Split('\"');
                    row[2] = RStrArr[1];

                    // Version
                    RStr = strArr[4];
                    RStrArr = RStr.Split('\"');
                    row[3] = RStrArr[1];
                }

                // Bind the Master to the dataset.
                MasterBS.DataSource = Tbl;
                MasterBS.DataMember = "DCFList";

                // Set Datasoure to Grid
                DataGVM.DataSource = MasterBS;
            }
        }

        private void BtnOptions_Click(object sender, EventArgs e)
        {
            // CustomerOneCentDiffRounding = Perform Tax / Local Amount Rounding
            // CustomerRTN5CentsCS = 5 Cents Rounding (Cash Sales)
            // CustomerRTN5CentsIV = 5 Cents Rounding (Sales Invoice)
            string lSQL;
            dynamic lDataSet;

            // Create Com Server object
            CheckLogin();

            // Get Option
            lSQL = "SELECT RNAME, RVALUE FROM SY_REGISTRY ";
            lSQL = lSQL + "WHERE RNAME IN ('CustomerOneCentDiffRounding', ";
            lSQL = lSQL + "'CustomerRTN5CentsCS', 'CustomerRTN5CentsIV') ";


            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            Tbl.Tables.Clear();
            PrepareData(lDataSet, "Master");

            // Bind the Master to the dataset.
            MasterBS.DataSource = Tbl;
            MasterBS.DataMember = "Master";

            // Set Datasoure to Grid
            DataGVM.DataSource = MasterBS;

            // Logout after done 
            FreeBiz(lDataSet);
            Logout();
        }

        private void btnPost2_Click(object sender, EventArgs e)
        {
            dynamic lMain, lDtl, BizObject, lQty,
                    lFld_MDockey, lFld_DocNo, lFld_Code, lFld_DocDate, lFld_PostDate, lFld_TaxDate,
                    lFld_Desc, lFld_Cancelled,
                    lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_ItemCode, lFld_Acc, lFld_Qty, lFld_Tax,
                    lFld_TaxRate, lFld_TaxInc, lFld_UnitPrice, lFld_TaxAmt;

            int I, J, R;
            DateTime lDate = new DateTime(DateTime.Now.Year, 1, 1);
            Random IP;

            edLogout.Checked = true;            
            for (I = 1; (I <= edRecord.Value); I++)
            {
                if (IsOdd(I))
                {
                    CheckLogin(0);
                }
                else
                {
                    CheckLogin(1);
                }

                BizObject = ComServer.BizObjects.Find(edBizType.Text);
                lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data
                lDtl = BizObject.DataSets.Find("cdsDocDetail");  //lDetail contains detail data

                lFld_MDockey = lMain.FindField("DOCKEY");
                lFld_DocNo = lMain.FindField("DocNo");
                lFld_Code = lMain.FindField("CODE");
                lFld_DocDate = lMain.FindField("DocDate");
                lFld_PostDate = lMain.FindField("PostDate");
                lFld_TaxDate = lMain.FindField("TAXDATE");
                lFld_Desc = lMain.FindField("Description");
                lFld_Cancelled = lMain.FindField("Cancelled");

                lFld_DDockey = lDtl.FindField("DOCKEY");
                lFld_DtlKey = lDtl.FindField("DTLKEY");
                lFld_Seq = lDtl.FindField("SEQ");
                lFld_ItemCode = lDtl.FindField("ItemCode");
                lFld_Acc = lDtl.FindField("Account");
                lFld_Qty = lDtl.FindField("QTY");
                lFld_Tax = lDtl.FindField("TAX");
                lFld_TaxRate = lDtl.FindField("TAXRATE");
                lFld_TaxInc = lDtl.FindField("TAXINCLUSIVE");
                lFld_UnitPrice = lDtl.FindField("UNITPRICE");
                lFld_TaxAmt = lDtl.FindField("TaxAmt");

                lbTime.Text = "Posting Record No : " + I;
                BizObject.New();
                //Begin Append Master
                lFld_MDockey.Value = -1;
                lFld_DocNo.AsString = String.Format(edFmt.Text, I);

                IP = new Random();
                R = IP.Next(0, edCoCode.Lines.Count());
                lFld_Code.AsString = edCoCode.Lines.GetValue(R);

                if (I >= 10)
                    lDate = lDate.AddDays(I / (double)10);
                lFld_DocDate.Value = lDate;
                lFld_PostDate.Value = lDate;
                lFld_TaxDate.Value = lDate;
                lFld_Desc.AsString = "Sales";
                lFld_Cancelled.Value = "F";
                lMain.Post();

                // Begin Append Detail
                for (J = 1; J <= edRecordItem.Value; J++)
                {
                    lDtl.Append();
                    lFld_DtlKey.Value = -1;
                    lFld_DDockey.Value = -1;
                    lFld_Seq.Value = J;

                    IP = new Random();
                    R = IP.Next(0, edItem.Lines.Count());
                    lFld_ItemCode.AsString = edItem.Lines.GetValue(R);

                    IP = new Random();
                    lQty = IP.Next(1, 10);

                    // lFld_Acc.AsString = "500-000"; //Sales Account Code  & can ignore if had itemcode
                    lFld_Qty.AsFloat = lQty;

                    if (edWithTax.Checked)
                    {
                        lFld_Tax.AsString = "SR";
                        lFld_TaxRate.AsString = "6%";
                        lFld_TaxInc.Value = 0;
                    }
                    else
                    {
                        lFld_Tax.AsString = "";
                        lFld_TaxRate.AsString = "";
                        lFld_TaxInc.Value = 0;
                    }

                    lFld_UnitPrice.AsFloat = 100;
                    if (edWithTax.Checked)
                        lFld_TaxAmt.AsFloat = 0.06 * (lQty * 100); // TaxRate * (Qty * UnitPrice Excluding Tax)
                    else
                        lFld_TaxAmt.AsFloat = 0;
                    lDtl.Post();
                }
                BizObject.Save();
                BizObject.Close();
                FreeBiz(BizObject);                
            }
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logout();
        }

        private void btnPost3_Click(object sender, EventArgs e)
        {
            dynamic lMain, lDtl, BizObject, lQty,
                    lFld_MDockey, lFld_DocNo, lFld_Code, lFld_DocDate, lFld_PostDate, lFld_TaxDate,
                    lFld_Desc, lFld_Cancelled,
                    lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_ItemCode, lFld_Acc, lFld_Qty, lFld_Tax,
                    lFld_TaxRate, lFld_TaxInc, lFld_UnitPrice, lFld_TaxAmt;

            int I, J, R;
            DateTime lDate = new DateTime(DateTime.Now.Year, 1, 1);
            Random IP;

            edLogout.Checked = true;
            for (I = 1; (I <= edRecord.Value); I++)
            {
                if (IsOdd(I))
                {
                    CheckLogin(0, 0);
                }
                else
                {
                    CheckLogin(1, 1);
                }

                BizObject = ComServer.BizObjects.Find(edBizType.Text);
                lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data
                lDtl = BizObject.DataSets.Find("cdsDocDetail");  //lDetail contains detail data

                lFld_MDockey = lMain.FindField("DOCKEY");
                lFld_DocNo = lMain.FindField("DocNo");
                lFld_Code = lMain.FindField("CODE");
                lFld_DocDate = lMain.FindField("DocDate");
                lFld_PostDate = lMain.FindField("PostDate");
                lFld_TaxDate = lMain.FindField("TAXDATE");
                lFld_Desc = lMain.FindField("Description");
                lFld_Cancelled = lMain.FindField("Cancelled");

                lFld_DDockey = lDtl.FindField("DOCKEY");
                lFld_DtlKey = lDtl.FindField("DTLKEY");
                lFld_Seq = lDtl.FindField("SEQ");
                lFld_ItemCode = lDtl.FindField("ItemCode");
                lFld_Acc = lDtl.FindField("Account");
                lFld_Qty = lDtl.FindField("QTY");
                lFld_Tax = lDtl.FindField("TAX");
                lFld_TaxRate = lDtl.FindField("TAXRATE");
                lFld_TaxInc = lDtl.FindField("TAXINCLUSIVE");
                lFld_UnitPrice = lDtl.FindField("UNITPRICE");
                lFld_TaxAmt = lDtl.FindField("TaxAmt");

                lbTime.Text = "Posting Record No : " + I;
                BizObject.New();
                //Begin Append Master
                lFld_MDockey.Value = -1;
                lFld_DocNo.AsString = String.Format(edFmt.Text, I);

                IP = new Random();
                R = IP.Next(0, edCoCode.Lines.Count());
                lFld_Code.AsString = edCoCode.Lines.GetValue(R);

                if (I >= 10)
                    lDate = lDate.AddDays(I / (double)10);
                lFld_DocDate.Value = lDate;
                lFld_PostDate.Value = lDate;
                lFld_TaxDate.Value = lDate;
                lFld_Desc.AsString = "Sales";
                lFld_Cancelled.Value = "F";
                lMain.Post();

                // Begin Append Detail
                for (J = 1; J <= edRecordItem.Value; J++)
                {
                    lDtl.Append();
                    lFld_DtlKey.Value = -1;
                    lFld_DDockey.Value = -1;
                    lFld_Seq.Value = J;

                    IP = new Random();
                    R = IP.Next(0, edItem.Lines.Count());
                    lFld_ItemCode.AsString = edItem.Lines.GetValue(R);

                    IP = new Random();
                    lQty = IP.Next(1, 10);

                    // lFld_Acc.AsString = "500-000"; //Sales Account Code  & can ignore if had itemcode
                    lFld_Qty.AsFloat = lQty;

                    if (edWithTax.Checked)
                    {
                        lFld_Tax.AsString = "SR";
                        lFld_TaxRate.AsString = "6%";
                        lFld_TaxInc.Value = 0;
                    }
                    else
                    {
                        lFld_Tax.AsString = "";
                        lFld_TaxRate.AsString = "";
                        lFld_TaxInc.Value = 0;
                    }

                    lFld_UnitPrice.AsFloat = 100;
                    if (edWithTax.Checked)
                        lFld_TaxAmt.AsFloat = 0.06 * (lQty * 100); // TaxRate * (Qty * UnitPrice Excluding Tax)
                    else
                        lFld_TaxAmt.AsFloat = 0;
                    lDtl.Post();
                }
                BizObject.Save();
                BizObject.Close();
                FreeBiz(BizObject);
            }
            MessageBox.Show("Done", "Posting Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Logout();
        }

        private void btnAging_Click(object sender, EventArgs e)
        {
            // M1 - Month to Date Payment received
            // M2 - Reserve field
            // C1 - Current Month
            // C2 - 1 Month
            // C3 - 2 Months 
            // C4 - 3 Months
            // C5 - 4 Months 
            // C6 - 5 Months & above

            const string Quote = "\"";
            dynamic RptObject, lDataSet1, lDataSet2, lDateTo, lAgeData;
            DataColumn tblC1, tblC2;
            DataRelation drt;

            // Step 1: Create Com Server object
            CheckLogin();
            // Step 2: Find and Create the Report Objects
            RptObject = ComServer.RptObjects.Find("Customer.Aging.RO");
            // Step 3: Spool parameters
            //6 Months Aging
            lAgeData = "<?xml version=" + Quote + "1.0" + Quote + " standalone=" + Quote + "yes" + Quote + "?>  <DATAPACKET Version=" + Quote + "2.0" + Quote + "><METADATA><FIELDS>" + "<FIELD attrname=" + Quote + "ColumnNo" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/><FIELD attrname=" + Quote + "ColumnType" + Quote + " fieldtype=" + Quote + "string" + Quote + " WIDTH=" + Quote + "1" + Quote + "/>" + "<FIELD attrname=" + Quote + "Param1" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/><FIELD attrname=" + Quote + "Param2" + Quote + " fieldtype=" + Quote + "i4" + Quote + " required=" + Quote + "true" + Quote + "/>" + "<FIELD attrname=" + Quote + "IsLocal" + Quote + " fieldtype=" + Quote + "boolean" + Quote + "/><FIELD attrname=" + Quote + "HeaderScript" + Quote + " fieldtype=" + Quote + "bin.hex" + Quote + " SUBTYPE=" + Quote + "Text" + Quote + " WIDTH=" + Quote + "1" + Quote + "/>" + "</FIELDS><PARAMS/></METADATA><ROWDATA><ROW ColumnNo=" + Quote + "0" + Quote + " ColumnType=" + Quote + "" + Quote + " Param1=" + Quote + "0" + Quote + " Param2=" + Quote + "0" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "1" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "0" + Quote + " Param2=" + Quote + "0" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;Current Mth&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "2" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-1" + Quote + " Param2=" + Quote + "-1" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;1 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "3" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-2" + Quote + " Param2=" + Quote + "-2" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;2 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "4" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-3" + Quote + " Param2=" + Quote + "-3" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;3 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "5" + Quote + " ColumnType=" + Quote + "A" + Quote + " Param1=" + Quote + "-4" + Quote + " Param2=" + Quote + "-4" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;4 Months&apos;&#013;end;" + Quote + "/>" + "<ROW ColumnNo=" + Quote + "6" + Quote + " ColumnType=" + Quote + "B" + Quote + " Param1=" + Quote + "-999999" + Quote + " Param2=" + Quote + "-5" + Quote + " IsLocal=" + Quote + "FALSE" + Quote + " HeaderScript=" + Quote + "ObjectPascal&#013;begin&#013;Value:= &apos;5 Month &amp; above&apos;&#013;end;" + Quote + "/>" + "</ROWDATA></DATAPACKET>";
            //lAgeData = richTextBox1.Text; //for 12 Months Aging
            RptObject.Params.Find("ActualGroupBy").Value = "Code;CompanyName"; // Fixed
                                                                               // RptObject.Params.Find("AgentData").Value            =  'Not use if AllAgent is True
            RptObject.Params.Find("AgingData").Value = lAgeData; // Fixed

            lDateTo = DateTime.Parse("April 17, 2019");
            RptObject.Params.Find("AgingDate").Value = lDateTo;

            RptObject.Params.Find("AgingOn").Value = "I"; // Fixed
            RptObject.Params.Find("AllAgent").Value = true;
            RptObject.Params.Find("AllArea").Value = true;
            RptObject.Params.Find("AllCompany").Value = true;
            RptObject.Params.Find("AllCompanyCategory").Value = true;
            RptObject.Params.Find("AllControlAccount").Value = true;
            RptObject.Params.Find("AllCurrency").Value = true;
            RptObject.Params.Find("AllDocProject").Value = true;
            // RptObject.Params.Find("AreaData").Value             =  'Not use if AllArea is True
            // RptObject.Params.Find("CompanyCategoryData").Value  =  'Not use if AllCompanyCategory is True
            // RptObject.Params.Find("CompanyData").Value = "300-A0001" + Environment.NewLine + "300-C0001" //Filter by Customer Code 300-A0001 & 300-C0001
            // RptObject.Params.Find("ControlAccountData").Value   =  'Not use if AllControlAccount is True
            // RptObject.Params.Find("CurrencyData").Value         =  'Not use if AllCurrency is True
            // RptObject.Params.Find("DocProjectData").Value       =  'Not use if AllDocProject is True
            RptObject.Params.Find("FilterPostDate").Value = false;
            // RptObject.Params.Find("GroupBy").Value               = 'Not use 
            RptObject.Params.Find("IncludePDC").Value = false;
            RptObject.Params.Find("IncludeZeroBalance").Value = false;
            RptObject.Params.Find("SortBy").Value = "Code;CompanyName";
            RptObject.Params.Find("DateTo").Value = lDateTo;

            // Step 4: Perform Report calculation 
            RptObject.CalculateReport();
            lDataSet1 = RptObject.DataSets.Find("cdsMain");
            lDataSet2 = RptObject.DataSets.Find("cdsDocument");
            //MessageBox.Show("Count - Main " + lDataSet1.RecordCount, "Count", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //MessageBox.Show("Count - Document " + lDataSet2.RecordCount, "Count", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 5 : Convert to Dataset
            ClearTables();

            PrepareData(lDataSet1, "Master");
            PrepareData(lDataSet2, "Detail");

            // Step 6 : Get Relation column
            tblC1 = Tbl.Tables["Master"].Columns["Code"];
            tblC2 = Tbl.Tables["Detail"].Columns["Code"];
            drt = new DataRelation("relation", tblC1, tblC2);


            // Step 7 : Set Grid Info
            SetGridInfo(drt);
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            Logout();
            RptObject = null;
        }

        private void BtnGetAgent_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDocKey;            

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("Agent");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data

            //Step 4: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);
            BizObject.Params.Find("CODE").Value = lDocKey;

            //Step 5 : Show Result
            if (Convert.IsDBNull(lDocKey))
            {
                edDesc.Text = "No Record Found";
            }
            else
            {
                BizObject.Open();
                edDesc.Text = lMain.FindField("DESCRIPTION").value;
            }
            //Step 6: Close
            BizObject.Close();

            //Step 7 : Logout after done                         
            FreeBiz(BizObject);
            Logout();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDocKey;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("Agent");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");    //lMain contains master data

            //Step 4: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);
            try
            {
                try
                {
                    //Step 5 : Insert or Update
                    if (Convert.IsDBNull(lDocKey))
                    {
                        BizObject.New();
                        lMain.FindField("CODE").value = edCode.Text;
                        lMain.FindField("DESCRIPTION").value = edDesc.Text;
                    }
                    else
                    {//Edit Data if found
                        BizObject.Params.Find("CODE").Value = lDocKey;
                        BizObject.Open();
                        BizObject.Edit();
                        lMain.FindField("DESCRIPTION").value = edDesc.Text;
                    }
                    //Step 6: Save & Close
                    BizObject.Save();
                    MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            finally
            {
                BizObject.Close();

                //Step 7 : Logout after done             
                FreeBiz(BizObject);
                Logout();
            }            
        }

        private void BtnAddSKU_Click(object sender, EventArgs e)
        {
            dynamic BizObject, lMain, lDtl, lDocKey;

            CheckLogin(0);
            //'Step 2: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("ST_ITEM");

            //Step 3: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  //lMain contains master data
            lDtl = BizObject.DataSets.Find("cdsUOM");        //lDtl contains UOM data

            //Step 4: Search
            lDocKey = BizObject.FindKeyByRef("CODE", edCode.Text);

            //Step 5 : Insert or Update
            if (Convert.IsDBNull(lDocKey))
            {
                BizObject.New();
                lMain.FindField("CODE").value = edCode.Text;
                lMain.FindField("DESCRIPTION").value = edDesc.Text;
                lMain.FindField("STOCKGROUP").value = "DEFAULT";
                lMain.FindField("STOCKCONTROL").value = "T";
                lMain.FindField("ISACTIVE").value = "T";

                lDtl.Edit(); //For 1St UOM
                lDtl.FindField("UOM").AsString = "PCS";
                lDtl.FindField("Rate").AsFloat = 1;
                lDtl.FindField("RefCost").AsFloat = 10.2;
                lDtl.FindField("RefPrice").AsFloat = 25;
                lDtl.Post();

                lDtl.Append(); //For 2nd UOM
                lDtl.FindField("UOM").AsString = "CTN";
                lDtl.FindField("Rate").AsFloat = 12;
                lDtl.FindField("RefCost").AsFloat = 102;
                lDtl.FindField("RefPrice").AsFloat = 240;
                lDtl.Post();
            }
            else
            {//Edit Data if found
                BizObject.Params.Find("Dockey").Value = lDocKey;
                BizObject.Open();
                BizObject.Edit();
                lMain.FindField("DESCRIPTION").value = edDesc.Text;

                //Delete all Detail
                while (lDtl.RecordCount > 0)
                {
                    lDtl.First();
                    lDtl.Delete();
                }

                //Insert back with new Price
                lDtl.Append(); //For 1St UOM
                lDtl.FindField("UOM").AsString = "PCS"; //Make sure this always same as b4 delete data
                lDtl.FindField("Rate").AsFloat = 1;     //Make sure this always same as b4 delete data
                lDtl.FindField("RefCost").AsFloat = 22.3;
                lDtl.FindField("RefPrice").AsFloat = 52;
                lDtl.Post();

                lDtl.Append(); //For 2nd UOM
                lDtl.FindField("UOM").AsString = "CTN"; //Make sure this always same as b4 delete data
                lDtl.FindField("Rate").AsFloat = 12;    //Make sure this always same as b4 delete data
                lDtl.FindField("RefCost").AsFloat = 102.5;
                lDtl.FindField("RefPrice").AsFloat = 260.45;
                lDtl.Post();
            }
            //Step 6: Save & Close
            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Step 7 : Logout after done             
            FreeBiz(BizObject);
            Logout();
        }

        private void btnSLIVList_Click(object sender, EventArgs e)
        {
            dynamic RptObject, lDateFrom, lDateTo, lDataSet1, lDataSet2;
            DataColumn tblC1, tblC2;
            DataRelation drt;
            string lSQL, s;

            // Step 1: Create Com Server object
            CheckLogin();
            //Filter Project
            lSQL = "SELECT Code FROM PROJECT ";
            lSQL = lSQL + "WHERE CODE NOT LIKE 'P12%'";
            lDataSet1 = ComServer.DBManager.NewDataSet(lSQL);

            s = "";
            lDataSet1.First();
            while ((!lDataSet1.Eof))
            {
                s = s + Environment.NewLine + lDataSet1.FindField("Code").AsString;
                lDataSet1.Next();
            }

            // Step 2: Find and Create the Report Objects
            RptObject = ComServer.RptObjects.Find("Sales.IV.RO");
            // Step 3: Spool parameters
            //RptObject.Params.Find("AgentData").Value              = //Not use if AllAgent is true
            RptObject.Params.Find("AllAgent").Value = true;
            RptObject.Params.Find("AllArea").Value = true;
            RptObject.Params.Find("AllCompany").Value = false;
            RptObject.Params.Find("AllCurrency").Value = true;
            RptObject.Params.Find("AllDocProject").Value = false;
            RptObject.Params.Find("AllDocument").Value = true;
            RptObject.Params.Find("AllItem").Value = true;
            RptObject.Params.Find("AllItemProject").Value = true;
            RptObject.Params.Find("AllLocation").Value = true;
            RptObject.Params.Find("AllStockGroup").Value = true;
            RptObject.Params.Find("AllCompanyCategory").Value = true;
            RptObject.Params.Find("AllBatch").Value = true;
            if (lBuildNo >= 776)
            {
                RptObject.Params.Find("AllTariff").Value = true;
               //RptObject.Params.Find("TariffData").Value         = //Not use if AllTariff is true
            }
            //RptObject.Params.Find("AreaData").Value               = //Not use if AllArea is true
            //RptObject.Params.Find("CompanyCategoryData").Value    = //Not use if AllCompanyCategory is true
            RptObject.Params.Find("CompanyData").Value = "300-A0003"; //"300-C0001"
                                                                      //RptObject.Params.Find("CurrencyData").Value           = //Not use if AllCurrency is true

            lDateFrom = DateTime.Parse("January 01, 2020");
            lDateTo = DateTime.Parse("December 31, 2020");

            RptObject.Params.Find("DateFrom").Value = lDateFrom;
            RptObject.Params.Find("DateTo").Value = lDateTo;
            RptObject.Params.Find("DocProjectData").Value = s;//Not use if AllDocProject is true
            //RptObject.Params.Find("DocumentData").Value            = //Not use if AllDocument is true
            //RptObject.Params.Find("GroupBy").Value                 = //If you wanted to grouping the data
            //RptObject.Params.Find("CategoryData").Value            = //Not use if HasCategory is false
            //RptObject.Params.Find("CategoryTpl").Value             = //For Internal use only
            RptObject.Params.Find("IncludeCancelled").Value = false;
            RptObject.Params.Find("HasCategory").Value = false;
            //RptObject.Params.Find("ItemData").Value                = //Not use if AllItem is true
            //RptObject.Params.Find("ItemProjectData").Value         = //Not use if AllItemProject is true
            //RptObject.Params.Find("LocationData").Value            = //Not use if AllLocation is true
            //RptObject.Params.Find("ItemCategoryData").Value        = //For Internal use only
            //RptObject.Params.Find("BatchData").Value               = //Not use if AllBatch is true
            RptObject.Params.Find("PrintDocumentStyle").Value = false;
            RptObject.Params.Find("SelectDate").Value = true;
            RptObject.Params.Find("SortBy").Value = "PostDate;DocNo;Code";
            //RptObject.Params.Find("StockGroupData").Value          = //Not use if AllStockGroup is true

            //Step 4: Perform Report calculation 
            RptObject.CalculateReport();

            lDataSet1 = RptObject.DataSets.Find("cdsMain");
            lDataSet2 = RptObject.DataSets.Find("cdsDocDetail");

            // Step 5 : Convert to Dataset
            ClearTables();

            PrepareData(lDataSet1, "Master");
            PrepareData(lDataSet2, "Detail");

            // Step 6 : Get Relation column
            tblC1 = Tbl.Tables["Master"].Columns["Dockey"];
            tblC2 = Tbl.Tables["Detail"].Columns["Dockey"];
            drt = new DataRelation("relation", tblC1, tblC2);


            // Step 7 : Set Grid Info
            SetGridInfo(drt);
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            //Logout();
            RptObject = null;
        }

        private void BtnOSSO_Click(object sender, EventArgs e)
        {
            dynamic RptObject, lDateFrom, lDateTo, lDataSet1, lDataSet2, lDataSet3;
            DataColumn tblmC1, tblmC2, tbldC1, tbldC2;
            DataRelation drt;

            // Step 1: Create Com Server object
            CheckLogin();
            // Step 2: Find and Create the Report Objects
            RptObject = ComServer.RptObjects.Find("Sales.OutstandingSO.RO");
            // Step 3: Spool parameters
            // RptObject.Params.Find("AgentData").Value            = 'Not use if AllAgent is true
            // RptObject.Params.Find("CompanyCategoryData").Value  = 'Not use if AllCompanyCategory is true
            // RptObject.Params.Find("LocationData").Value         = 'Not use if AllLocation is true
            // RptObject.Params.Find("StockGroupData").Value       = 'Not use if AllStockGroup is true
            RptObject.Params.Find("AllAgent").Value = true;
            RptObject.Params.Find("AllArea").Value = true;
            RptObject.Params.Find("AllCompany").Value = true;
            RptObject.Params.Find("AllDocument").Value = false;
            RptObject.Params.Find("AllItem").Value = true;
            RptObject.Params.Find("AllItemProject").Value = true;
            // RptObject.Params.Find("AreaData").Value              = 'Not use if AllArea is true
            // RptObject.Params.Find("CompanyData").Value           = 'Not use if AllCompany is true

            lDateFrom = DateTime.Parse("January 01, 2019");
            lDateTo = DateTime.Parse("December 31, 2019");

            // RptObject.Params.Find("DateFrom").Value              = lDateFrom
            // RptObject.Params.Find("DateTo").Value                = lDateTo
            // RptObject.Params.Find("DeliveryDateFrom").Value      = lDateFrom
            // RptObject.Params.Find("DeliveryDateTo").Value        = lDateTo
            RptObject.Params.Find("DocumentData").Value = "SO-00130" + Environment.NewLine + "SO-00131";
            // RptObject.Params.Find("GroupBy").Value               = 'If you wanted to grouping the data
            RptObject.Params.Find("IncludeCancelled").Value = false;
            // RptObject.Params.Find("ItemData").Value               = 'Not use if AllItem is true
            RptObject.Params.Find("PrintFulfilledItem").Value = true; // Print transfered info
            RptObject.Params.Find("PrintOutstandingItem").Value = true; // Print untransfer info
                                                                        // RptObject.Params.Find("ItemProjectData").Value       = 'Not use if AllItemProject is true
            RptObject.Params.Find("SelectDate").Value = false;
            RptObject.Params.Find("SelectDeliveryDate").Value = false;
            RptObject.Params.Find("SortBy").Value = "DocDate;DocNo;Code";
            RptObject.Params.Find("AllDocProject").Value = true;
            RptObject.Params.Find("AllLocation").Value = true;
            RptObject.Params.Find("AllCompanyCategory").Value = true;
            RptObject.Params.Find("AllBatch").Value = true;
            RptObject.Params.Find("HasCategory").Value = false;
            RptObject.Params.Find("AllStockGroup").Value = true;
            // RptObject.Params.Find("CategoryData").Value          = 'For Internal use only
            // RptObject.Params.Find("CategoryTpl").Value           = 'For Internal use only
            // RptObject.Params.Find("ItemCategoryData").Value      = 'For Internal use only
            // RptObject.Params.Find("DocProjectData").Value        = 'Not use if AllDocProject is true
            // RptObject.Params.Find("BatchData").Value             = 'Not use if AllBatch is true
            if (lBuildNo >= 776)
                RptObject.Params.Find("AllTariff").Value = true;
            RptObject.Params.Find("TranferDocFilterDate").Value = false;
            // Step 4: Perform Report calculation 
            RptObject.CalculateReport();

            lDataSet1 = RptObject.DataSets.Find("cdsMain");// The outstanding Qty in Smallest UOM
            lDataSet2 = RptObject.DataSets.Find("cdsTransfer"); // Shown Transfer Qty in Smallest UOM

            // To find Actual Outstanding Qty UOM -> Use OutstandingQty (in cdsMain)/Rate (in cdsDocDetail)
            // Link key for cdsMain & cdsDocDetail is Dtlkey
            lDataSet3 = RptObject.DataSets.Find("cdsDocDetail");

            // Step 5 : Convert to Dataset
            ClearTables();

            PrepareData(lDataSet1, "Master");
            PrepareData(lDataSet2, "Detail");

            // Step 6 : Get Relation column
            tblmC1 = Tbl.Tables["Master"].Columns["Dockey"];
            tblmC2 = Tbl.Tables["Master"].Columns["Dtlkey"];
            tbldC1 = Tbl.Tables["Detail"].Columns["FromDocKey"];
            tbldC2 = Tbl.Tables["Detail"].Columns["FromDtlKey"];
            DataColumn[] tblM = new DataColumn[] { tblmC1, tblmC2 };
            DataColumn[] tblD = new DataColumn[] { tbldC1, tbldC2 };


            drt = new DataRelation("relation", tblM, tblD);

            // Step 7 : Set Grid Info
            SetGridInfo(drt);

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done 
            Logout();
            RptObject = null;
        }

        private void btnGLJEEdit_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet, BizObject, lMain, lDetail;
            
            // Step 1: Create Com Server object
            CheckLogin();
            
            // Step 2: GetDockey
            lSQL = "SELECT Dockey FROM GL_JE ";
            lSQL = lSQL + "WHERE DocNo=" + QuotedStr(edDocNo.Text); // JV-00002
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            // Step 3: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("GL_JE");

            // Step 4: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data  

            // Step 5 : Find Doc Number
            if (lDataSet.RecordCount > 0)
            {
                lDataSet.First();
                BizObject.Params.Find("DocKey").Value = lDataSet.FindField("DocKey").AsString;

                BizObject.Open();
                BizObject.Edit();
                lMain.Edit();
                lMain.FindField("Description").AsString = "Edited Description 123";

                // Step 6: Delete all Detail
                while (lDetail.RecordCount != 0)
                {
                    lDetail.First();
                    lDetail.Delete();
                }

                // Step 7: Append Detail
                lDetail.Append();
                lDetail.FindField("DtlKey").value = -1;
                lDetail.FindField("Code").value = edCode.Text;
                lDetail.FindField("Description").value = "testing desc1";
                lDetail.FindField("Project").value = "P12W1";
                lDetail.FindField("Tax").value = "";
                lDetail.FindField("TaxInclusive").value = 0;
                lDetail.FindField("LocalDR").value = 200;
                lDetail.FindField("DR").value = 200;
                lDetail.Post();

                lDetail.Append();
                lDetail.FindField("DtlKey").value = -1;
                lDetail.FindField("Code").value = edCode.Text;
                lDetail.FindField("Description").value = "testing desc2";
                lDetail.FindField("Project").value = "P13W1";
                lDetail.FindField("Tax").value = "";
                lDetail.FindField("TaxInclusive").value = 0;
                lDetail.FindField("LocalCR").value = 200;
                lDetail.FindField("CR").value = 200;
                lDetail.Post();

                lDetail.Append();
                lDetail.FindField("DtlKey").value = -1;
                lDetail.FindField("Code").value = edCode.Text;
                lDetail.FindField("Description").value = "testing desc3";
                lDetail.FindField("Project").value = "P12W1";
                lDetail.FindField("Tax").value = "";
                lDetail.FindField("TaxInclusive").value = 0;
                lDetail.FindField("LocalDR").value = 500.1;
                lDetail.FindField("DR").value = 500.1;
                lDetail.Post();

                lDetail.Append();
                lDetail.FindField("DtlKey").value = -1;
                lDetail.FindField("Code").value = edCode.Text;
                lDetail.FindField("Description").value = "testing desc4";
                lDetail.FindField("Project").value = "P13W1";
                lDetail.FindField("Tax").value = "";
                lDetail.FindField("TaxInclusive").value = 0;
                lDetail.FindField("LocalCR").value = 500.1;
                lDetail.FindField("CR").value = 500.1;
                lDetail.Post();

                // Step 8: Save Document
                BizObject.Save();
                BizObject.Close();

                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Record Not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Step 9 : Logout after done  
            FreeBiz(lDataSet);
            FreeBiz(BizObject);
            Logout();
        }

        private void btnSLCSEdit_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet, BizObject, lMain, lDetail;

            // Step 1: Create Com Server object
            CheckLogin();

            // Step 2: GetDockey
            lSQL = "SELECT Dockey FROM SL_CS ";
            lSQL = lSQL + "WHERE DocNo=" + QuotedStr(edDocNo.Text);  // CS-00002
            lSQL = lSQL + " AND Code='300-K0001' ";
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            // Step 3: Find and Create the Biz Objects
            BizObject = ComServer.BizObjects.Find("SL_CS");

            // Step 4: Set Dataset
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDetail = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data  

            // Step 5 : Find Doc Number
            if (lDataSet.RecordCount > 0)
            {
                lDataSet.First();
                BizObject.Params.Find("DocKey").Value = lDataSet.FindField("DocKey").AsString;
                try
                {
                    BizObject.Open();
                    BizObject.Edit();
                    lMain.Edit();
                    lMain.FindField("Description").AsString = "Edited Description 123";

                    // Step 6: Delete all Detail
                    while (lDetail.RecordCount != 0)
                    {
                        lDetail.First();
                        lDetail.Delete();
                    }
                    // Step 7: Append Detail
                    lDetail.Append();
                    lDetail.FindField("DtlKey").value = -1;
                    lDetail.FindField("Account").value = "500-0000";
                    lDetail.FindField("Description").value = "Item A";
                    lDetail.FindField("Tax").value = "";
                    lDetail.FindField("TaxInclusive").value = 0;
                    lDetail.FindField("Amount").value = 410.37;
                    lDetail.FindField("TaxAmt").value = 0;
                    lDetail.Post();

                // Step 8: Save Document

                    BizObject.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                BizObject.Close();

                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("Record Not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // Step 9 : Logout after done  
            FreeBiz(lDataSet);
            FreeBiz(BizObject);
            Logout();
        }

        private void btnNextNo_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet, lNextNo;
            DateTime lDate;
            lDate = DateTime.Today;

            // Step 1: Create Com Server object
            CheckLogin();

            // Step 2: Get DocNo
            lSQL = "SELECT A.*, B.NEXTNUMBER FROM SY_DOCNO A ";
            lSQL = lSQL + "INNER JOIN SY_DOCNO_DTL B ON (A.DOCKEY=B.PARENTKEY) ";
            lSQL = lSQL + "WHERE A.DOCTYPE='IV' ";
            lSQL = lSQL + "AND A.DESCRIPTION='Customer Invoice' ";
            lSQL = lSQL + "AND A.STATESET=1 ";

            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            if (lDataSet.RecordCount > 0)
            {
                lDataSet.First();
                edDesc.Text = lDataSet.FindField("Description").AsString;
                edCode.Text = lDataSet.FindField("Format").AsString;
                lNextNo = lDataSet.FindField("NextNumber").Value;

                if (edCode.Text.Contains("{") == true)  // Convert from eg IV-{@YYMM}-%.5d to IV-202102-{0:d5}
                {
                    if (edCode.Text.Contains("YYYY") == true)
                    {
                        edCode.Text = edCode.Text.Replace("YYYY", lDate.ToString("yyyy"));
                    }
                    if (edCode.Text.Contains("YY") == true)
                    {
                        edCode.Text = edCode.Text.Replace("YY", lDate.ToString("yy"));
                    }
                    if (edCode.Text.Contains("MM") == true)
                    {
                        edCode.Text = edCode.Text.Replace("MM", lDate.ToString("MM"));
                    }
                    edCode.Text = edCode.Text.Replace("@", "");
                    edCode.Text = edCode.Text.Replace("{", "");
                    edCode.Text = edCode.Text.Replace("}", "");
                }

                // Convert from eg IV-%.5d to IV-{0:d5}
                edCode.Text = edCode.Text.Replace("d", "");
                edCode.Text = edCode.Text.Replace("%.", "{0:d") + "}";
                edDocNo.Text = string.Format(edCode.Text, lNextNo);

                FreeBiz(lDataSet);
                Logout();
            }
            else
                MessageBox.Show("Record Not Found", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnValidSKU_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet;
            bool B;

            // Create Com Server object
            CheckLogin();

            // Query Data
            lSQL = "SELECT UOM FROM ST_ITEM_UOM WHERE CODE= " + QuotedStr(edCode.Text);
            lSQL = lSQL + "AND UOM= " + QuotedStr(edDesc.Text);
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            B = lDataSet.RecordCount > 0;

            edDocNo.Text = B ? "Is Valid Item Code/SKU" : edCode.Text + " is Not Valid Item Code/SKU";

            // Logout after done 
            FreeBiz(lDataSet);
            Logout();
        }

        private void BtnValidGLAcc_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet;
            bool B;

            // Create Com Server object
            CheckLogin();

            // Query Data
            lSQL = "SELECT A.Dockey, A.Parent, A.Code, A.Description, A.SpecialAccType FROM GL_ACC A ";
            lSQL = lSQL + "LEFT OUTER JOIN GL_ACC B ON (A.Dockey=B.Parent) ";
            lSQL = lSQL + "WHERE A.Parent<>-1 ";
            lSQL = lSQL + "AND B.Dockey IS NULL ";
            lSQL = lSQL + "AND A.SpecialAccType NOT IN('DC', 'CC') ";
            lSQL = lSQL + "AND A.CODE= " + QuotedStr(edCode.Text);
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            B = lDataSet.RecordCount > 0;

            edDocNo.Text = B ? "Is Valid Account Code" : edCode.Text + " is Not Valid Account Code";

            // Logout after done 
            FreeBiz(lDataSet);
            Logout();
        }

        private void BtnSTItem_Click(object sender, EventArgs e)
        {
            dynamic lDataSet;
            string lSQL;

            // Step 1: Login
            CheckLogin();
            
            lSQL = "SELECT A.*, B.UOM, B.RATE, B.REFCOST, B.REFPRICE, B.ISBASE FROM ST_ITEM A ";
            lSQL = lSQL + "INNER JOIN ST_ITEM_UOM B ON (A.CODE=B.CODE) ";
            lSQL = lSQL + "WHERE A.ISACTIVE='T' ";

            if (edCode.Text != "")
            {
                // Begin if wanted filter by Supplier Code
                lSQL = lSQL + "AND EXISTS (SELECT CODE FROM ST_ITEM_COMPANY C ";
                lSQL = lSQL + "              WHERE CTYPE='S' ";
                lSQL = lSQL + "              AND COMPANY=" + QuotedStr(edCode.Text);
                lSQL = lSQL + "              AND C.CODE=A.CODE) ";
            }

            lSQL = lSQL + "ORDER BY A.CODE, B.RATE";

            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            Tbl.Tables.Clear();
            PrepareData(lDataSet, "Master");

            // Bind the Master to the dataset.
            MasterBS.DataSource = Tbl;
            MasterBS.DataMember = "Master";

            // Set Datasoure to Grid
            DataGVM.DataSource = MasterBS;

            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 8 : Logout after done
            FreeBiz(lDataSet);
            Logout();
        }

        private void BtnMthEndFifo_Click(object sender, EventArgs e)
        {
            // Accuracy : 90% - Due to the figure is base on last run Costing in SQL Accounting
            dynamic lSQL, lDataSet1, lDataset2, lDataset3;
            string s, lDateTo;
            DataColumn tblC1, tblC2;
            DataRelation drt;

            // Step 1: Create Com Server object
            CheckLogin();

            lDateTo = "'31 Dec 2017' ";
            // Step 2: GetMaxSeq
            lSQL = "SELECT  A.ItemCode, A.Location, A.Batch, MAX(B.Seq) AS Seq, 1 AS CostingMethod ";
            lSQL = lSQL + "FROM ST_TR A INNER JOIN ST_TR_FIFO B ON (A.TRANSNO=B.TRANSNO) ";
            lSQL = lSQL + "WHERE A.PostDate<=" + lDateTo;
            lSQL = lSQL + "And A.ITEMCODE =" + QuotedStr(edCode.Text);
            lSQL = lSQL + " GROUP BY A.ItemCode, A.Location, A.Batch";
            lDataSet1 = ComServer.DBManager.NewDataSet(lSQL);

            s = "";
            lDataSet1.First();
            while ((!lDataSet1.Eof))
            {
                s = s + lDataSet1.FindField("SEQ").AsString + ",";
                lDataSet1.Next();
            }
            s = s.Remove(s.Length - 1); // Remove last String

            // Step 3: Get Balance Qty & Up To Date Total Cost
            lSQL = "SELECT A.TRANSNO, A.ItemCode, A.Location, A.Batch, B.QTY, B.COST FROM ST_TR A ";
            lSQL = lSQL + "INNER JOIN ST_TR_FIFO B ON (A.TRANSNO=B.TRANSNO) ";
            lSQL = lSQL + "WHERE B.COSTTYPE='U' ";
            lSQL = lSQL + "AND A.PostDate<=" + lDateTo;
            lSQL = lSQL + "And A.ITEMCODE =" + QuotedStr(edCode.Text);
            lSQL = lSQL + " AND B.SEQ IN (" + s + ") ";
            lSQL = lSQL + " AND B.QTY<>0 ";
            lSQL = lSQL + "ORDER BY A.ItemCode, A.Location, A.Batch";
            lDataset2 = ComServer.DBManager.NewDataSet(lSQL);

            s = "";
            lDataset2.First();
            while ((!lDataset2.Eof))
            {
                s = s + lDataset2.FindField("TRANSNO").AsString + ",";
                lDataset2.Next();
            }
            s = s.Remove(s.Length - 1); // Remove last String

            // Step 4 : Get FIFO Detail
            lSQL = "SELECT  TransNo, Cost, SUM(Qty) AS Qty, MIN(CostSeq) AS Seq FROM ST_TR_FIFO ";
            lSQL = lSQL + "WHERE CostType='B' ";
            lSQL = lSQL + "And ITEMCODE =" + QuotedStr(edCode.Text);
            lSQL = lSQL + " AND TRANSNO IN (" + s + ") ";
            lSQL = lSQL + " GROUP BY TransNo, Cost";

            lDataset3 = ComServer.DBManager.NewDataSet(lSQL);

            PrepareData(lDataset2, "Master");
            PrepareData(lDataset3, "Detail");

            // Step 6 : Get Relation column
            tblC1 = Tbl.Tables["Master"].Columns["TransNo"];
            tblC2 = Tbl.Tables["Detail"].Columns["TransNo"];
            drt = new DataRelation("relation", tblC1, tblC2);

            // Step 7 : Set Grid Info
            SetGridInfo(drt);

            // Step 8 : Logout after done 
            FreeBiz(lDataSet1);
            FreeBiz(lDataset2);
            FreeBiz(lDataset3);
            Logout();
        }

        private void BtnMthEndWA_Click(object sender, EventArgs e)
        {
            // Accuracy : 90% - Due to the figure is base on last run Costing in SQL Accounting
            dynamic lSQL, lDataSet1, lDataset2;
            string s, lDateTo;

            // Step 1: Create Com Server object
            CheckLogin();
            lDateTo = "'31 Dec 2017'";
            // Step 2: GetMaxSeq
            lSQL = "SELECT A.ItemCode, A.Location, A.Batch,  MAX(B.Seq) AS Seq, 2 AS CostingMethod ";
            lSQL = lSQL + "FROM ST_TR A INNER JOIN ST_TR_WMA B ON (A.TRANSNO=B.TRANSNO) ";
            lSQL = lSQL + "WHERE A.PostDate<=" + lDateTo;
            lSQL = lSQL + "And A.ITEMCODE =" + QuotedStr(edCode.Text);
            lSQL = lSQL + " GROUP BY A.ItemCode, A.Location, A.Batch";
            lDataSet1 = ComServer.DBManager.NewDataSet(lSQL);

            s = "";
            lDataSet1.First();
            while ((!lDataSet1.Eof))
            {
                s = s + lDataSet1.FindField("SEQ").AsString + ",";
                lDataSet1.Next();
            }
            s = s.Remove(s.Length - 1); // Remove last String

            // Step 3: Get Balance Qty & Up To Date Total Cost
            lSQL = "SELECT A.TRANSNO, A.ItemCode, A.Location, A.Batch, B.UTDQty, B.UTDCost FROM ST_TR A ";
            lSQL = lSQL + "INNER JOIN ST_TR_WMA B ON (A.TRANSNO=B.TRANSNO) ";
            lSQL = lSQL + "WHERE A.PostDate<=" + lDateTo;
            lSQL = lSQL + "And A.ITEMCODE =" + QuotedStr(edCode.Text);
            lSQL = lSQL + " AND B.SEQ IN (" + s + ") ";
            lSQL = lSQL + " AND B.UTDQty<>0 ";
            lSQL = lSQL + "ORDER BY A.ItemCode, A.Location, A.Batch";
            lDataset2 = ComServer.DBManager.NewDataSet(lSQL);

            Tbl.Tables.Clear();
            PrepareData(lDataset2, "Master");

            // Bind the Master to the dataset.
            MasterBS.DataSource = Tbl;
            MasterBS.DataMember = "Master";

            // Set Datasoure to Grid
            DataGVM.DataSource = MasterBS;

            // Step 7 : Logout after done 
            FreeBiz(lDataSet1);
            FreeBiz(lDataset2);
            Logout();
        }

        private void BtnSTAS_Click(object sender, EventArgs e)
        {
            dynamic lDate, lMain, lDtl, BizObject, lFld_MDockey, lFld_DocNo, 
                    lFld_Code, lFld_DocDate, lFld_PostDate, lFld_MLoc, lFld_Desc, 
                    lFld_Cancelled, lFld_MQty, lFld_MUOM, lFld_DDockey, lFld_DtlKey, 
                    lFld_Seq, lFld_ItemCode, lFld_Qty, lFld_Wastage;

            CheckLogin(0);

            BizObject = ComServer.BizObjects.Find("ST_AS");
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDtl = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data

            lFld_MDockey = lMain.FindField("DOCKEY");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_Code = lMain.FindField("ITEMCODE");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Desc = lMain.FindField("Description");
            lFld_Cancelled = lMain.FindField("Cancelled");
            lFld_MLoc = lMain.FindField("LOCATION");
            lFld_MQty = lMain.FindField("QTY");
            lFld_MUOM = lMain.FindField("UOM");

            lFld_DDockey = lDtl.FindField("DOCKEY");
            lFld_DtlKey = lDtl.FindField("DTLKEY");
            lFld_Seq = lDtl.FindField("SEQ");
            lFld_ItemCode = lDtl.FindField("ItemCode");
            lFld_Qty = lDtl.FindField("QTY");
            lFld_Wastage = lDtl.FindField("ISWASTAGE");


            lDate = DateTime.Parse("January 1, 2019");

            BizObject.New();
            // Begin Append Master
            lFld_MDockey.Value = -1;
            lFld_DocNo.AsString = "--AS Test--";
            lFld_Code.AsString = "BOM";
            lFld_DocDate.Value = lDate;
            lFld_PostDate.Value = lDate;
            lFld_Desc.AsString = "BOM Description 123";
            lFld_Cancelled.AsString = "F";
            lFld_MLoc.AsString = "KL";
            lFld_MQty.AsFloat = 2;
            lFld_MUOM.AsString = "UNIT";

            // Delete all Detail
            while (lDtl.RecordCount != 0)
            {
                lDtl.First();
                lDtl.Delete();
            }

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 1;
            lFld_ItemCode.AsString = "ANT";
            lFld_Qty.AsFloat = 3;
            lFld_Wastage.AsString = "F";
            lDtl.Post();

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 2;
            lFld_ItemCode.AsString = "ANT";
            lFld_Qty.AsFloat = 2;
            lFld_Wastage.AsString = "T";
            lDtl.Post();

            lDtl.Append();
            lFld_DtlKey.Value = -1;
            lFld_DDockey.Value = -1;
            lFld_Seq.Value = 3;
            lFld_ItemCode.AsString = "COVER";
            lFld_Qty.AsFloat = 3;
            lFld_Wastage.AsString = "F";
            lDtl.Post();

            BizObject.Save();
            BizObject.Close();
            MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            FreeBiz(BizObject);
            Logout();
        }

        private void btnJO2AS_Click(object sender, EventArgs e)
        {
            dynamic lDate, lMain, lDtl, BizObject, lxFer, lSQL, 
                    lFld_MDockey, lFld_DocNo, 
                    lFld_Code, lFld_DocDate, lFld_PostDate, lFld_MLoc, lFld_Desc, 
                    lFld_Cancelled, lFld_MQty, lFld_MUOM, lFld_MItemCode, lFld_FromDocType, 
                    lFld_FromDockey, lFld_DDockey, lFld_DtlKey, lFld_Seq, lFld_ItemCode, 
                    lFld_Qty, lFld_Wastage;

            CheckLogin(0);
            // Insert PD JO
            BizObject = ComServer.BizObjects.Find("PD_JO");
            lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
            lDtl = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data

            lFld_MDockey = lMain.FindField("DOCKEY");
            lFld_DocNo = lMain.FindField("DocNo");
            lFld_Code = lMain.FindField("CODE");
            lFld_MItemCode = lMain.FindField("ITEMCODE");
            lFld_DocDate = lMain.FindField("DocDate");
            lFld_PostDate = lMain.FindField("PostDate");
            lFld_Desc = lMain.FindField("Description");
            lFld_Cancelled = lMain.FindField("Cancelled");
            lFld_MLoc = lMain.FindField("LOCATION");
            lFld_MQty = lMain.FindField("QTY");
            lFld_MUOM = lMain.FindField("UOM");

            lFld_DDockey = lDtl.FindField("DOCKEY");
            lFld_DtlKey = lDtl.FindField("DTLKEY");
            lFld_Seq = lDtl.FindField("SEQ");
            lFld_ItemCode = lDtl.FindField("ItemCode");
            lFld_Qty = lDtl.FindField("QTY");
            lFld_Wastage = lDtl.FindField("ISWASTAGE");

            lDate = DateTime.Parse("January 2, 2019");

            lbTime.Text = "Posting Job Order...";
            BizObject.New();
            // Begin Append Master
            lFld_MDockey.Value = -1;
            lFld_DocNo.AsString = "--JO Test--";
            lFld_Code.AsString = "300-A0003";
            lFld_MItemCode.AsString = "BOM";
            lFld_DocDate.Value = lDate;
            lFld_PostDate.Value = lDate;
            lFld_Desc.AsString = "BOM Description JO";
            lFld_Cancelled.AsString = "F";
            lFld_MLoc.AsString = "KL";
            lFld_MQty.AsFloat = 2;
            lFld_MUOM.AsString = "UNIT";

            BizObject.Save();
            BizObject.Close();

            // Check Is Transfered or not
            lbTime.Text = "Check Transfer Status...";
            lSQL = "SELECT DocKey FROM ST_AS ";
            lSQL = lSQL + "WHERE FromDocKey IN (SELECT DocKey FROM PD_JO ";
            lSQL = lSQL + "WHERE DocNo='--JO Test--') ";
            lSQL = lSQL + "AND FromDocType='JO' ";
            lxFer = ComServer.DBManager.NewDataSet(lSQL);

            if (lxFer.RecordCount == 0)
            {
                lSQL = "SELECT DocKey FROM PD_JO ";
                lSQL = lSQL + "WHERE DocNo='--JO Test--' ";
                lxFer = ComServer.DBManager.NewDataSet(lSQL);

                // Post ST AS
                BizObject = ComServer.BizObjects.Find("ST_AS");
                lMain = BizObject.DataSets.Find("MainDataSet");  // lMain contains master data
                lDtl = BizObject.DataSets.Find("cdsDocDetail"); // lDetail contains detail data

                lFld_MDockey = lMain.FindField("DOCKEY");
                lFld_DocNo = lMain.FindField("DocNo");
                lFld_MItemCode = lMain.FindField("ITEMCODE");
                lFld_DocDate = lMain.FindField("DocDate");
                lFld_PostDate = lMain.FindField("PostDate");
                lFld_Desc = lMain.FindField("Description");
                lFld_Cancelled = lMain.FindField("Cancelled");
                lFld_MLoc = lMain.FindField("LOCATION");
                lFld_MQty = lMain.FindField("QTY");
                lFld_MUOM = lMain.FindField("UOM");
                lFld_FromDocType = lMain.FindField("FromDocType");
                lFld_FromDockey = lMain.FindField("FromDocKey");

                lFld_DDockey = lDtl.FindField("DOCKEY");
                lFld_DtlKey = lDtl.FindField("DTLKEY");
                lFld_Seq = lDtl.FindField("SEQ");
                lFld_ItemCode = lDtl.FindField("ItemCode");
                lFld_Qty = lDtl.FindField("QTY");
                lFld_Wastage = lDtl.FindField("ISWASTAGE");

                lDate = DateTime.Parse("January 1, 2019");

                lbTime.Text = "Transfer JO to AS...";
                BizObject.New();
                // Begin Append Master
                lFld_MDockey.Value = -1;
                lFld_DocNo.AsString = "--AS Test--";
                lFld_MItemCode.AsString = "BOM";
                lFld_DocDate.Value = lDate;
                lFld_PostDate.Value = lDate;
                lFld_Desc.AsString = "BOM Description AS";
                lFld_Cancelled.AsString = "F";
                lFld_MLoc.AsString = "KL";
                lFld_MQty.AsFloat = 2;
                lFld_MUOM.AsString = "UNIT";
                lFld_FromDocType.AsString = "JO";
                lFld_FromDockey.AsFloat = lxFer.FindField("DocKey").AsFloat;

                // Delete all Detail
                while (lDtl.RecordCount != 0)
                {
                    lDtl.First();
                    lDtl.Delete();
                }

                lDtl.Append();
                lFld_DtlKey.Value = -1;
                lFld_DDockey.Value = -1;
                lFld_Seq.Value = 1;
                lFld_ItemCode.AsString = "ANT";
                lFld_Qty.AsFloat = 3;
                lFld_Wastage.AsString = "F";
                lDtl.Post();

                lDtl.Append();
                lFld_DtlKey.Value = -1;
                lFld_DDockey.Value = -1;
                lFld_Seq.Value = 2;
                lFld_ItemCode.AsString = "ANT";
                lFld_Qty.AsFloat = 2;
                lFld_Wastage.AsString = "T";
                lDtl.Post();

                lDtl.Append();
                lFld_DtlKey.Value = -1;
                lFld_DDockey.Value = -1;
                lFld_Seq.Value = 3;
                lFld_ItemCode.AsString = "COVER";
                lFld_Qty.AsFloat = 3;
                lFld_Wastage.AsString = "F";
                lDtl.Post();

                BizObject.Save();
                BizObject.Close();
                MessageBox.Show("Done", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FreeBiz(BizObject);
                FreeBiz(lxFer);
                Logout();
                lbTime.Text = "";
            }
        }

        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }

        private void BtnRtf_Click(object sender, EventArgs e)
        {
            dynamic lSQL, lDataSet;

            // Create Com Server object
            CheckLogin();

            // Query Data
            lSQL = "SELECT Description3, Picture FROM ST_ITEM WHERE Code= " + QuotedStr(edCode.Text);
            lDataSet = ComServer.DBManager.NewDataSet(lSQL);

            edDesc.Text = lDataSet.FindField("Description3").AsString;
           //edCoCode.Text = lDataSet.FindField("Description3").AsString;
            pictureBox1.Image = ByteToImage(lDataSet.FindField("Picture").Value);
            richTextBox1.Rtf = edDesc.Text;
            // Logout after done 
            FreeBiz(lDataSet);
            Logout();
        }
    }
}
