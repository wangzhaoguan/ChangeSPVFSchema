namespace ChangeSPVFSchema
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    public class ProcDefForm : Form
    {
        private Button btnChangeSchema;
        private Button btnLink;
        private IContainer components;
        private DbConnection conn;
        private string dbCataLog = "";
        private string dbConnString = "";
        private string dboldUser = "";
        private string dbPsw = "";
        private string dbServerName = "";
        private string dbUser = "";
        private DataSet dsContent;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label lblProcName;
        private TextBox txtdbCataLog;
        private TextBox txtDbPsw;
        private TextBox txtDbServer;
        private TextBox txtDbUser;
        private TextBox txtOldUser;

        public ProcDefForm()
        {
            this.InitializeComponent();
        }

        private void btnChangeSchema_Click(object sender, EventArgs e)
        {
            try
            {
                this.txtOldUser.Enabled = false;
                this.txtOldUser.Refresh();
                this.btnLink.Enabled = false;
                this.btnChangeSchema.Enabled = false;
                if ((this.txtOldUser.Text == null) || (this.txtOldUser.Text.Trim() == ""))
                {
                    MessageBox.Show("请设置原来的属主");
                    this.txtOldUser.Focus();
                }
                else
                {
                    this.dboldUser = this.txtOldUser.Text.Trim();
                    if (this.ValidRefresh())
                    {
                        this.lblProcName.Text = "正在更改函数、视图、存储过程、触发器中的属主....请稍后";
                        this.lblProcName.Refresh();
                        //DataSet set = this.ExecSqlWithReturn("select name  AS SPName,sys.sql_modules.definition  AS SPDef    from sys.procedures,sys.sql_modules where sys.procedures.object_id=sys.sql_modules.object_id and sys.sql_modules.definition  like '%" + this.txtOldUser.Text.Trim() + ".%'  ORDER BY SPNAME ", true);
//select t.name, t.type, s.definition
//    , case  when type = 'FN' then 1
//            when type = 'TF' then 1
//            when type = 'V'  then 2
//            when type = 'P'  then 3
//            else 99 end 
//      as idx
//from sys.objects t
//    join sys.sql_modules s on t.object_id = s.object_id
//where t.type IN ('FN', 'TF', 'P', 'V', 'TR')
//   and s.definition like '%LCGS609999.%'
//order by idx, t.type;

                        string sqlQuery = @"select t.name SPName, t.type, s.definition SPDef, case when type = 'FN' then 1 when type = 'TF' then 1 when type = 'V'  then 2 when type = 'P'  then 3 else 99 end as idx from sys.objects t join sys.sql_modules s on t.object_id = s.object_id where t.type IN ('FN', 'TF', 'P', 'V', 'TR') ";
                        sqlQuery +=  " and s.definition like '%" + this.txtOldUser.Text.Trim() + ".%'";
                        sqlQuery += " order by idx, t.type ";
                        DataSet set = this.ExecSqlWithReturn(sqlQuery, true);
                        if (((set == null) || (set.Tables.Count < 1)) || (set.Tables[0].Rows.Count < 1))
                        {
                            this.Log("没有需要更改属主的函数、视图、存储过程、触发器");
                            MessageBox.Show("没有需要更改属主的函数、视图、存储过程、触发器");
                            this.lblProcName.Text = "就绪";
                            this.btnLink.Enabled = true;
                            this.btnChangeSchema.Enabled = true;
                            this.txtDbServer.Enabled = true;
                            this.txtdbCataLog.Enabled = true;
                            this.txtDbUser.Enabled = true;
                            this.txtDbPsw.Enabled = true;
                            this.txtOldUser.Enabled = true;
                            this.Refresh();
                        }
                        else
                        {
                            StringBuilder builder = null;
                            StringBuilder builder2 = null;
                            string str = "";
                            string objType = "";
                            this.lblProcName.Text = "正在更改函数、视图、存储过程、触发器中的属主....请稍后";
                            this.lblProcName.Refresh();
                            foreach (DataRow row in set.Tables[0].Rows)
                            {
                                str = Convert.ToString(row["SPName"]).Trim();
                                objType = Convert.ToString(row["type"]);
                                builder2 = new StringBuilder(Convert.ToString(row["SPDef"]));
                                builder = new StringBuilder(Convert.ToString(row["SPDef"]));
                                try
                                {
                                    builder = builder.Replace(this.dboldUser, this.txtDbUser.Text.Trim());
                                    builder = builder.Replace(this.dboldUser.ToLower(), this.txtDbUser.Text.Trim());
                                    builder = builder.Replace(this.dboldUser.ToUpper(), this.txtDbUser.Text.Trim());
                                    this.Log("准备更改" + row["SPName"] + "中的属主;");

                                    string psSql = string.Empty;
                                    switch (objType.Trim().ToUpper())
                                    {
                                        case "FN":
                                        case "TF":
                                            psSql = "if exists(select 1 from sysobjects where name = '" + str + "' )  drop function " + str + ";";
                                            this.ExecSqlWithoutReturn(psSql);
                                            break;
                                        case "V":
                                            psSql = "if exists(select 1 from sysobjects where name = '" + str + "' )  drop view " + str + ";";
                                            this.ExecSqlWithoutReturn(psSql);
                                            break;
                                        case "P":
                                            psSql = "if exists(select 1 from sysobjects where name = '" + str + "' )  drop proc " + str + ";";
                                            this.ExecSqlWithoutReturn(psSql);
                                            break;
                                        case "TR":
                                            psSql = "if exists(select 1 from sysobjects where name = '" + str + "' )  drop trigger " + str + ";";
                                            this.ExecSqlWithoutReturn(psSql);
                                            break;
                                        default:
                                            break;
                                    }
                                    this.ExecSqlWithoutReturn(builder.ToString());
                                    this.Log("更改了" + row["SPName"] + "中的属主;");
                                }
                                catch (Exception ex)
                                {
                                    this.Log("更新" + row["SPName"] + "中的属主出错;");
                                    this.Log(ex.ToString());
                                    this.Log("执行出错的数据库对象如下：");
                                    this.Log(builder.ToString());
                                    this.Log("数据库对象原有定义如下：");
                                    this.Log(builder2.ToString());
                                    this.Log("更新数据库对象中的属主出错, 更新过程中止。 修改错误后，请重新进行更改数据库对象属主的工作");
                                    MessageBox.Show("更新[" + row["SPName"] + @"]中的属主出错,更新过程中止。日志记录在C:\GSLog\GS_ChangeSchema.txt 中 ,请手工修改错误的数据库对象定义，并重新运行更改对象属主的工具", "警告");
                                    break;
                                }
                            }
                            this.Log("以上数据库对象中的属主由" + this.dboldUser + "更改为" + this.txtDbUser.Text.Trim() + "；已更改完毕。");
                            MessageBox.Show("数据库对象中的属主由" + this.dboldUser + "成功更改为" + this.txtDbUser.Text.Trim() + @"；日志记录在C:\GSLog\GS_ChangeSchema.txt中", "消息");
                            this.btnLink.Enabled = true;
                            this.btnChangeSchema.Enabled = true;
                            this.lblProcName.Text = "就绪";
                            this.lblProcName.Refresh();
                            base.Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            if (this.ValidLink())
            {
                DataSet set = null;
                this.dbConnString = "data source=" + this.dbServerName + ";initial catalog=" + this.dbCataLog + ";user id=" + this.dbUser + ";password=" + this.dbPsw + ";Connect Timeout=200";
                set = this.ExecSqlWithReturn("select getdate() ", true);
                if ((set != null) && (set.Tables.Count > 0))
                {
                    this.btnChangeSchema.Enabled = true;
                    this.Log("======================================================");
                    this.Log("数据库连接成功!  服务器:" + this.dbServerName + " 数据库：" + this.dbCataLog + " 登陆用户：" + this.dbUser + ";");
                    MessageBox.Show("数据库连接成功");
                    this.btnChangeSchema.Enabled = true;
                    this.lblProcName.Text = "已连接数据库";
                    this.txtdbCataLog.Enabled = false;
                    this.txtDbPsw.Enabled = false;
                    this.txtDbServer.Enabled = false;
                    this.txtDbUser.Enabled = false;
                    this.txtOldUser.Enabled = true;
                }
                else
                {
                    this.Log("数据库连接失败");
                    MessageBox.Show("数据库连接失败");
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (this.ValidRefresh())
            {
                this.dbConnString = "data source=" + this.dbServerName + ";initial catalog=master;user id=" + this.dbUser + ";password=" + this.dbPsw + ";Connect Timeout=200";
                this.dbConnString = "";
                this.lblProcName.Text = "数据库刷新成功";
            }
        }

        private void ClearDbLink(int piFlag)
        {
            this.dbServerName = "";
            this.dbCataLog = "";
            this.dbConnString = "";
            this.dbPsw = "";
            this.dbUser = "";
            this.conn = null;
        }

        private void CloseConn()
        {
            try
            {
                if ((this.conn != null) && (this.conn.State == ConnectionState.Open))
                {
                    this.conn.Close();
                }
            }
            catch
            {
            }
        }

        private void cmbDb_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ClearDbLink(1);
        }

        private void cmbDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ClearDbLink(0);
        }

        private bool CreateConn()
        {
            this.CloseConn();
            this.conn = new SqlConnection(this.dbConnString);
            try
            {
                this.conn.Open();
                return true;
            }
            catch (Exception exception)
            {
                this.Log("连接失败：" + exception.Message);
                MessageBox.Show("连接失败：" + exception.Message);
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ExecSqlWithoutReturn(string psSql)
        {
            try
            {
                if (this.CreateConn())
                {
                    DbCommand command = new SqlCommand();
                    command.CommandTimeout = 0x708;
                    command.Connection = this.conn;
                    command.CommandType = CommandType.Text;
                    command.CommandText = psSql;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                this.CloseConn();
            }
        }

        private DataSet ExecSqlWithReturn(string psSql, bool pbIsSql)
        {
            try
            {
                if (!this.CreateConn())
                {
                    return null;
                }
                DbDataAdapter adapter = new SqlDataAdapter(psSql, (SqlConnection) this.conn);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);
                return dataSet;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                this.CloseConn();
            }
            return null;
        }

        private StringBuilder GetProcDetail(string vsProcID)
        {
            string psSql = "";
            psSql = "select text from syscomments  where id = '" + vsProcID + "'";
            this.dsContent = this.ExecSqlWithReturn(psSql, true);
            return this.SetProcContent();
        }

        private void InitializeComponent()
        {
            this.btnLink = new System.Windows.Forms.Button();
            this.lblProcName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDbServer = new System.Windows.Forms.TextBox();
            this.txtDbUser = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDbPsw = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnChangeSchema = new System.Windows.Forms.Button();
            this.txtOldUser = new System.Windows.Forms.TextBox();
            this.txtdbCataLog = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnLink
            // 
            this.btnLink.Location = new System.Drawing.Point(287, 124);
            this.btnLink.Name = "btnLink";
            this.btnLink.Size = new System.Drawing.Size(60, 23);
            this.btnLink.TabIndex = 4;
            this.btnLink.Text = "连接";
            this.btnLink.UseVisualStyleBackColor = true;
            this.btnLink.Click += new System.EventHandler(this.btnLink_Click);
            // 
            // lblProcName
            // 
            this.lblProcName.AutoSize = true;
            this.lblProcName.Location = new System.Drawing.Point(12, 243);
            this.lblProcName.Name = "lblProcName";
            this.lblProcName.Size = new System.Drawing.Size(23, 12);
            this.lblProcName.TabIndex = 2;
            this.lblProcName.Text = "...";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "服务器名称：";
            // 
            // txtDbServer
            // 
            this.txtDbServer.Location = new System.Drawing.Point(94, 26);
            this.txtDbServer.Name = "txtDbServer";
            this.txtDbServer.Size = new System.Drawing.Size(187, 21);
            this.txtDbServer.TabIndex = 0;
            this.txtDbServer.Text = "(local)";
            // 
            // txtDbUser
            // 
            this.txtDbUser.Location = new System.Drawing.Point(94, 92);
            this.txtDbUser.Name = "txtDbUser";
            this.txtDbUser.Size = new System.Drawing.Size(187, 21);
            this.txtDbUser.TabIndex = 2;
            this.txtDbUser.Text = "LC****9999";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "登录用户名：";
            // 
            // txtDbPsw
            // 
            this.txtDbPsw.Location = new System.Drawing.Point(94, 125);
            this.txtDbPsw.Name = "txtDbPsw";
            this.txtDbPsw.PasswordChar = '*';
            this.txtDbPsw.Size = new System.Drawing.Size(187, 21);
            this.txtDbPsw.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "密码：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(25, 166);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "原有属主：";
            // 
            // btnChangeSchema
            // 
            this.btnChangeSchema.Enabled = false;
            this.btnChangeSchema.Location = new System.Drawing.Point(135, 200);
            this.btnChangeSchema.Name = "btnChangeSchema";
            this.btnChangeSchema.Size = new System.Drawing.Size(89, 27);
            this.btnChangeSchema.TabIndex = 6;
            this.btnChangeSchema.Text = "修改属主";
            this.btnChangeSchema.UseVisualStyleBackColor = true;
            this.btnChangeSchema.Click += new System.EventHandler(this.btnChangeSchema_Click);
            // 
            // txtOldUser
            // 
            this.txtOldUser.Location = new System.Drawing.Point(94, 162);
            this.txtOldUser.Name = "txtOldUser";
            this.txtOldUser.Size = new System.Drawing.Size(187, 21);
            this.txtOldUser.TabIndex = 5;
            this.txtOldUser.Text = "LC****9999";
            // 
            // txtdbCataLog
            // 
            this.txtdbCataLog.Location = new System.Drawing.Point(94, 59);
            this.txtdbCataLog.Name = "txtdbCataLog";
            this.txtdbCataLog.Size = new System.Drawing.Size(187, 21);
            this.txtdbCataLog.TabIndex = 1;
            this.txtdbCataLog.Text = "cwbase****";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 63);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "数据库名称：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(287, 166);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 15;
            this.label4.Text = "*区分大小写";
            // 
            // ProcDefForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 264);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtdbCataLog);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtOldUser);
            this.Controls.Add(this.btnChangeSchema);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtDbPsw);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDbUser);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtDbServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblProcName);
            this.Controls.Add(this.btnLink);
            this.Name = "ProcDefForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "属主更改小工具V1.6";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public void Log(string msg)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(@"C:\GSLog\GS_ChangeSchema");
            builder.Append(".txt");
            if (!Directory.Exists(@"C:\GSLog\"))
            {
                Directory.CreateDirectory(@"C:\GSLog\");
            }
            StreamWriter writer = new StreamWriter(builder.ToString(), true);
            writer.WriteLine(DateTime.Now.ToString() + " :" + msg);
            writer.Flush();
            writer.Close();
        }

        private void ProcDefForm_Load(object sender, EventArgs e)
        {
        }

        private StringBuilder SetProcContent()
        {
            if (((this.dsContent == null) || (this.dsContent.Tables.Count < 1)) || (this.dsContent.Tables[0].Rows.Count < 1))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            foreach (DataRow row in this.dsContent.Tables[0].Rows)
            {
                builder.Append(Convert.ToString(row["Text"]).Replace("\n", Environment.NewLine));
            }
            this.dsContent.Clear();
            this.dsContent = null;
            return builder;
        }

        private void txtDbPsw_TextChanged(object sender, EventArgs e)
        {
            this.ClearDbLink(0);
        }

        private void txtDbServer_TextChanged(object sender, EventArgs e)
        {
            this.ClearDbLink(0);
        }

        private void txtDbUser_TextChanged(object sender, EventArgs e)
        {
            this.ClearDbLink(0);
        }

        private bool ValidLink()
        {
            if (!this.ValidRefresh())
            {
                return false;
            }
            return true;
        }

        private bool ValidRefresh()
        {
            if ((this.txtDbServer.Text == null) || (this.txtDbServer.Text.Trim() == ""))
            {
                MessageBox.Show("请设置数据库服务名");
                this.txtDbServer.Focus();
                return false;
            }
            this.dbServerName = this.txtDbServer.Text;
            if ((this.txtDbUser.Text == null) || (this.txtDbUser.Text.Trim() == ""))
            {
                MessageBox.Show("请设置数据库登录用户");
                this.txtDbUser.Focus();
                return false;
            }
            this.dbUser = this.txtDbUser.Text;
            if ((this.txtDbPsw.Text == null) || (this.txtDbPsw.Text.Trim() == ""))
            {
                MessageBox.Show("请设置数据库登录密码");
                this.txtDbPsw.Focus();
                return false;
            }
            this.dbPsw = this.txtDbPsw.Text;
            if ((this.txtdbCataLog.Text == null) || (this.txtdbCataLog.Text.Trim() == ""))
            {
                MessageBox.Show("请设置数据库");
                this.txtdbCataLog.Focus();
                return false;
            }
            this.dbCataLog = this.txtdbCataLog.Text;
            return true;
        }
    }
}

