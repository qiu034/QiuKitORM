using Microsoft.VisualBasic.ApplicationServices;
using QiuKitCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.ListBox;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QiuKitORM.GUI
{
    public partial class QiuKitORM : Form
    {
        private string host = "";
        private string user = "";
        private string pwd = "";
        public QiuKitORM()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            ConnForm conn = new ConnForm();
            conn.SetData += Conn_GetData;
            conn.StartPosition = FormStartPosition.CenterParent;
            conn.ShowDialog();
        }


        private void Conn_GetData(string host, string user, string pwd, bool state)
        {
            if (state)
            {
                this.host = host;
                this.user = user;
                this.pwd = pwd;
                string connStr = string.Format("Data Source={0};Initial Catalog=master;User ID={1};password={2}", host, user, pwd);
                //加载所有数据库
                DataTable dt = SqlHelper.Instance.ExecuteDataset(connStr, "SELECT dbid,name FROM sysdatabases").Tables[0];
                if (null != dt)
                {
                    foreach (DataRow item in dt.Rows)
                    {
                        ListBoxDb.Items.Add(item["name"].ToString());
                    }
                }
            }
            else
            {
                ListBoxDb.Items.Clear();
                MessageBox.Show("连接失败！");
            }
        }

        private void ListBoxDb_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtTD.Text = "";
            if (ListBoxDb.SelectedItem == null) return;
            string dbName = ListBoxDb.SelectedItem.ToString();
            CreateSqlStr(dbName);
        }

        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridView1.Columns["table_name"].HeaderText = "数据表";
            dataGridView1.Columns["table_name"].Width = 400;
        }

        private void txtTD_TextChanged(object sender, EventArgs e)
        {
            if (ListBoxDb.SelectedItem == null) return;
            string dbName = ListBoxDb.SelectedItem.ToString();
            if (string.IsNullOrEmpty(txtTD.Text))
            {
                CreateSqlStr(dbName);
                return;
            }
            CreateSqlStr(dbName, txtTD.Text.Trim());
        }



        /// <summary>
        /// 查询所有表
        /// </summary>
        /// <param name="dbName"></param>
        private void CreateSqlStr(string dbName)
        {
            string connStr = string.Format("Data Source={0};Initial Catalog={1};User ID={2};password={3}", host, dbName, user, pwd);
            DataTable dt = SqlHelper.Instance.ExecuteDataset(connStr, "select table_name from information_schema.tables").Tables[0];
            if (null != dt)
            {
                dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.DataSource = dt;
            }
        }


        /// <summary>
        /// 模糊查询表
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="condition"></param>
        private void CreateSqlStr(string dbName, string condition)
        {
            string connStr = string.Format("Data Source={0};Initial Catalog={1};User ID={2};password={3}", host, dbName, user, pwd);
            DataTable dt = SqlHelper.Instance.ExecuteDataset(connStr, string.Format("select table_name from information_schema.tables  where table_name Like'%{0}%' ", condition)).Tables[0];
            if (null != dt)
            {
                dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.DataSource = dt;
            }
        }

        private void QiuKitORM_Load(object sender, EventArgs e)
        {
            //全选checkedListBox
            for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
            {
                this.checkedListBox1.SetItemChecked(i, true);
            }

            //获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            txtPath.Text = desktopPath;
        }


        /// <summary>
        /// 修改路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = folder.SelectedPath;
            }
        }


        /// <summary>
        /// 生成指定文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count <= 0)
            {
                MessageBox.Show("请选择要生成的文件类型！");
                return;
            }

            DataGridViewSelectedRowCollection selectedRow = this.dataGridView1.SelectedRows;
            if (selectedRow.Count < 0)
            {
                MessageBox.Show("请先选择数据表！");
                return;
            }

            string dbName = ListBoxDb.SelectedItem.ToString();
            for (int i = 0; i < selectedRow.Count; i++)
            {
                //生成Model
                if (checkedListBox1.GetItemChecked(0))
                {
                    var table = selectedRow[i].Cells[0].Value.ToString();
                    ExecSqlPrint(dbName,table, txtPath.Text + "\\" + table + "Model.cs");
                }

                //生成Repository
                if (checkedListBox1.GetItemChecked(1))
                {
                    var table = selectedRow[i].Cells[0].Value.ToString();
                    RepositoryPrint(table, txtPath.Text + "\\" + table + "Repository.cs");
                }

                //生成DAL
                if (checkedListBox1.GetItemChecked(2))
                {
                    var table = selectedRow[i].Cells[0].Value.ToString();
                    DALPrint(table, txtPath.Text + "\\" + table + "Repository.cs");
                }

            }
            MessageBox.Show("导出完成！");
        }

        #region 方法 -> 执行Sql,打印需求内容
        /// <summary>
        /// 执行Sql,打印需求内容
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <param name="tableName">数据表名</param>
        /// <param name="path">生成路径</param>
        private void ExecSqlPrint(string dbName,string tableName, string path)
        {
            string connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};password={3}", host, dbName, user, pwd);
            DataTable dt = SqlHelper.Instance.ExecuteDataset(connectionString, SqlHelper.Instance.GetModelStr(tableName)).Tables[0];
            StringBuilder result = new StringBuilder($"public class {tableName}Model \r\n");
            result.AppendLine("{");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.AppendLine($"    private {dt.Rows[i]["ColumnType"]} _{dt.Rows[i]["ColumnName"]};");
                result.AppendLine($"    public {dt.Rows[i]["ColumnType"]} {dt.Rows[i]["ColumnName"]}");
                result.AppendLine("    {");
                result.AppendLine($"        get{{ return _{dt.Rows[i]["ColumnName"]}; }}");
                result.AppendLine($"        set{{ _{dt.Rows[i]["ColumnName"]} = value; }}");
                result.AppendLine("    }");
                result.AppendLine("");
            }
            result.AppendLine("}");
            System.IO.File.AppendAllText(path, result.ToString());
        }
        #endregion

        #region 方法 -> 生成Reposiotry
        /// <summary>
        /// 生成Reposiotry
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="path">生成路径</param>
        private void RepositoryPrint(string tableName, string path)
        {
            StringBuilder result = new StringBuilder($"public class {tableName}Repository : BaseDAL<{tableName}Model> \r\n");
            result.AppendLine("{");
            result.AppendLine($@"    private const string table = ""{tableName}"";");
            result.AppendLine("}");
            System.IO.File.AppendAllText(path, result.ToString());
        }
        #endregion

        #region 方法 -> 生成DAL
        /// <summary>
        /// 生成DAL
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="path">生成路径</param>
        private void DALPrint(string tableName, string path)
        {
            StringBuilder result = new StringBuilder($"public class {tableName}DAL : BaseDAL<{tableName}Model> \r\n");
            result.AppendLine("{");
            result.AppendLine($@"    private const string table = ""{tableName}"";");
            result.AppendLine("}");
            System.IO.File.AppendAllText(path, result.ToString());
        }
        #endregion
    }
}
