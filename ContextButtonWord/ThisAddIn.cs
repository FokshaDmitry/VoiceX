using System;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Core;
using System.Threading;
using System.Windows.Forms;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;

namespace ContextButtonWord
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Word.Application wordApp = this.Application;

            CommandBar commandBars = Application.CommandBars["Text"];
            foreach (CommandBarControl control in commandBars.Controls)
            {
                if (control is CommandBarButton && ((CommandBarButton)control).Tag == "MyButtonTag")
                {
                    control.Delete();
                }
            }
            CommandBarButton button = (CommandBarButton)commandBars.Controls.Add(
                MsoControlType.msoControlButton,
                missing, 
                missing, 
                missing, 
                true);   

            button.Caption = "Call Number";
            button.Tag = "MyButtonTag";
            button.Click += new _CommandBarButtonEvents_ClickEventHandler(MyButton_Click);
        }
        private void MyButton_Click(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            try
            {
                Word.Application excelApp = this.Application;
                var selectedRange = excelApp.Selection;
                var value = selectedRange.Text;
                if (!String.IsNullOrEmpty(value))
                {
                    Thread.Sleep(1000);
                    SendInSystray(value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async Task SendInSystray(string phone)
        {
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "VoiceXPipe", PipeDirection.Out))
                {
                    if (!pipeClient.IsConnected)
                    {
                        await pipeClient.ConnectAsync();
                    }
                    using (StreamWriter sw = new StreamWriter(pipeClient))
                    {
                        await sw.WriteLineAsync(phone);
                        await sw.FlushAsync();
                        sw.Close();
                        sw.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
