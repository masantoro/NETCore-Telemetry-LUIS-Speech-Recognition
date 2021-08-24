using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using SpeechForm.Business.Attendance;
using SpeechForm.Models.Attendance.Enum;
using SpeechForm.Models.Attendance;
using SpeechForm.Repository.Entity.Table;
using SpeechForm.Repository.Enum;
using System.Threading.Tasks;
using SpeechForm.Repository;

namespace SpeechForm.Forms
{
    public partial class Atendimento : Form
    {
        private ServiceManager _servicesAttendant;
        private ServiceManager _servicesCustomer;
        private Call call = new Call();

        //Call Session
        private CallManager callManager;

        private ServiceOperation serviceOperation;


        public Atendimento(ServiceOperation ServiceOperation)
        {
            Singleton.ResetConfiguration();

            callManager = new CallManager();

            serviceOperation = ServiceOperation;

            InitializeComponent();

            lblServiceOperation.Text = serviceOperation.ServiceOperationDescription;
            lblSupervisor.Text = $"{serviceOperation.SupervisorName} [{serviceOperation.SupervisorUsername}]";
            lblAttendant.Text = $"{serviceOperation.AttendantName} [{serviceOperation.AttendantUsername}]";

            //Call Session
            call.CallStart = DateTime.UtcNow;
            call.CallId = Guid.NewGuid().ToString();
            call.ServiceOperation = serviceOperation;
            Task.Run(() => callManager.SaveCall(call, serviceOperation));

            _servicesAttendant = new ServiceManager();
            // Iniciar transcricao do atendente
            call.ChannelAtendente.channel = Speech.Channel.Atendente;
            call.ChannelAtendente.SentimentoScore_Changed += ChannelAtendente_SentimentoScore_Changed;
            call.ChannelAtendente.Sentimento_Changed += ChannelAtendente_Sentimento_Changed;
            call.ChannelAtendente.Intent_Changed += ChannelAtendente_Intent_Changed;
            call.ChannelAtendente.IntentScore_Changed += ChannelAtendente_IntentScore_Changed;
            call.ChannelAtendente.Transcricao_Changed += ChannelAtendente_Transcricao_Changed;
            call.ChannelAtendente.CallId = call.CallId;
            call.ChannelAtendente.CallStart = call.CallStart;
            _servicesAttendant.Speech(call, call.ChannelAtendente);

            // Iniciar transcricao do cliente
            var _loopback = new CaptureLoopBack();
            _loopback.startLoopBack();

            _servicesCustomer = new ServiceManager();
            call.ChannelCliente.channel = Speech.Channel.Cliente;
            call.ChannelCliente.SentimentoScore_Changed += ChannelCliente_SentimentoScore_Changed;
            call.ChannelCliente.Sentimento_Changed += ChannelCliente_Sentimento_Changed;
            call.ChannelCliente.Intent_Changed += ChannelCliente_Intent_Changed;
            call.ChannelCliente.IntentScore_Changed += ChannelCliente_IntentScore_Changed;
            call.ChannelCliente.Transcricao_Changed += ChannelCliente_Transcricao_Changed;
            call.ChannelCliente.CallId = call.CallId;
            call.ChannelCliente.CallStart = call.CallStart;
            _servicesCustomer.Speech(call, call.ChannelCliente);

        }

        private void SetTextOnThread(TextBox obj, string Text)
        {
            obj.Invoke((MethodInvoker)delegate { obj.Text = Text; });
        }

        private void AddTextOnThread(TextBox obj, string Text)
        {
            obj.Invoke((MethodInvoker)delegate { obj.Text += Text; });
        }

        private void SetBackColorOnThread(TextBox obj, Color BackColor)
        {
            obj.Invoke((MethodInvoker)delegate { obj.BackColor = BackColor; });
        }

        private void ChannelCliente_Transcricao_Changed(string Text)
        {
            AddTextOnThread(txtTranscricaoCliente, Text);
        }

        private void ChannelCliente_IntentScore_Changed(string Text)
        {
            SetTextOnThread(txtIntentScoreCliente, Text);
        }

        private void ChannelCliente_Intent_Changed(string Text)
        {
            SetTextOnThread(txtIntentCliente, Text);
        }

        private void ChannelCliente_Sentimento_Changed(string Text)
        {
            SetTextOnThread(txtSentimentoCliente, Text);
        }

        private void ChannelCliente_SentimentoScore_Changed(string Text, Color BackColor)
        {
            SetTextOnThread(txtSentimentoScoreCliente, Text);
            SetBackColorOnThread(txtSentimentoScoreCliente, BackColor);
        }

        private void ChannelAtendente_Transcricao_Changed(string Text)
        {
            AddTextOnThread(txtTranscricaoAtendente, Text);
        }

        private void ChannelAtendente_IntentScore_Changed(string Text)
        {
            SetTextOnThread(txtIntentScoreAtendente, Text);
        }

        private void ChannelAtendente_Intent_Changed(string Text)
        {
            SetTextOnThread(txtIntentAtendente, Text);
        }

        private void ChannelAtendente_Sentimento_Changed(string Text)
        {
            SetTextOnThread(txtSentimentoAtendente, Text);
        }

        private void ChannelAtendente_SentimentoScore_Changed(string Text, Color BackColor)
        {
            SetTextOnThread(txtSentimentoScoreAtendente, Text);
            SetBackColorOnThread(txtSentimentoScoreAtendente, BackColor);
        }

        private void Atendimento_Load(object sender, EventArgs e)
        {

        }

        private void txtTranscricaoCliente_TextChanged(object sender, EventArgs e)
        {
            txtTranscricaoCliente.SelectionStart = txtTranscricaoCliente.Text.Length;
            txtTranscricaoCliente.ScrollToCaret();
        }

        private void txtTranscricaoAtendente_TextChanged(object sender, EventArgs e)
        {
            txtTranscricaoAtendente.SelectionStart = txtTranscricaoAtendente.Text.Length;
            txtTranscricaoAtendente.ScrollToCaret();
        }

        private void Atendimento_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Deseja fechar a aplicação?", "Atendimento", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            } 
        }

        private void Atendimento_Closed(object sender, System.EventArgs e)
        {
            //Application.ExitThread();
            _servicesAttendant.StopService();
            _servicesCustomer.StopService();
            //callManager.EndCall().Wait();
            Task.Run(() => callManager.EndCall());
            Application.Exit();
            Environment.Exit(0);
        }

    }
}
