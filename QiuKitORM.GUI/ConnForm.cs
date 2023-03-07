using QiuKitCore;

namespace QiuKitORM.GUI
{
    public partial class ConnForm : Form
    {
        public delegate void SetValue(string host, string user, string pwd, bool state);
        public event SetValue? SetData;
        public ConnForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text;
            string user = txtUser.Text;
            string pwd = txtPwd.Text;
            string connStr = string.Format("Data Source={0};Initial Catalog=master;User ID={1};password={2}", host, user, pwd);

            try
            {
                if (SqlHelper.Instance.ConnTest(host, user, pwd))
                {
                    MessageBox.Show("连接成功！");
                    SetData(host, user, pwd, true);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("连接失败！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接失败！\r\n" + ex);
            }


        }
    }
}
