using SpeechForm.Models.Attendance;
using SpeechForm.Repository.Entity.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SpeechForm.Forms
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            string Version = "2.0";
            lblVersion.Text = $"Versão {Version}";

            txtUser.Focus();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //Application.ExitThread();
            //Application.Exit();
            Environment.Exit(0);
        }

        private void disabledControls(bool disabled)
        {
            Cursor.Current = disabled ? Cursors.WaitCursor : Cursors.Default;
            pnlLoading.Visible = disabled;
            Application.DoEvents();
            txtUser.Enabled = !disabled;
            txtPassword.Enabled = !disabled;
            btnExit.Enabled = !disabled;
            btnLogin.Enabled = !disabled;
            Application.DoEvents();
        }

        private void CheckEnterKeyPress(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                CheckLogin();
                ((TextBox)sender).Focus();
            }
        }

        private void CheckLogin()
        {
            disabledControls(true);
            if (String.IsNullOrEmpty(txtUser.Text) || String.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Preencher usuário e senha!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var attendant = (new UserEntity()).Filtered<UserEntity>(x => x.Username.ToUpper().Trim() == txtUser.Text.ToUpper().Trim() && x.Password == txtPassword.Text).FirstOrDefault();
                if (attendant == null)
                {
                    MessageBox.Show("Usuário e/ou senha inválidos!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    var serviceOperationAttendant = (new ServiceOperationAttendantEntity()).Filtered<ServiceOperationAttendantEntity>(x => x.AttendantId == attendant.UserId).FirstOrDefault();
                    if (serviceOperationAttendant == null)
                    {
                        MessageBox.Show("Este usuário não é um Atendente!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        var serviceOperation = (new ServiceOperationEntity()).Filtered<ServiceOperationEntity>(x => x.ServiceOperationId == serviceOperationAttendant.ServiceOperationId).FirstOrDefault();
                        if (serviceOperation == null)
                        {
                            MessageBox.Show("Operação inexistente para este Atendente!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            var supervisor = (new UserEntity()).Filtered<UserEntity>(x => x.UserId == serviceOperation.SupervisorId).FirstOrDefault();
                            if (supervisor == null)
                            {
                                MessageBox.Show("Supervisor inexistente!", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else
                            {

                                var serviceOperationModel = new ServiceOperation
                                {
                                    ServiceOperationId = serviceOperation.ServiceOperationId,
                                    ServiceOperationDescription = serviceOperation.Description,
                                    SupervisorId = supervisor.UserId,
                                    SupervisorName = supervisor.Name,
                                    SupervisorUsername = supervisor.Username,
                                    AttendantId = attendant.UserId,
                                    AttendantName = attendant.Name,
                                    AttendantUsername = attendant.Username
                                };

                                var atendimento = new Atendimento(serviceOperationModel);
                                atendimento.Show();
                                this.Hide();
                            }
                        }

                    }
                }
            }
            disabledControls(false);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            CheckLogin();
        }
    }
}
