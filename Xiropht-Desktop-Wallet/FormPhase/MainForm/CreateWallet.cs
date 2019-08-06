﻿#if WINDOWS
using MetroFramework;
#endif
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xiropht_Connector_All.Setting;
using Xiropht_Connector_All.Wallet;
using Xiropht_Wallet.Wallet;

namespace Xiropht_Wallet.FormPhase.MainForm
{
    public partial class CreateWallet : Form
    {
        public bool InCreation;

        public CreateWallet()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams CP = base.CreateParams;
                CP.ExStyle = CP.ExStyle | 0x02000000; // WS_EX_COMPOSITED
                return CP;
            }
        }

        public void GetListControl()
        {
            if (Program.WalletXiropht.ListControlSizeCreateWallet.Count == 0)
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    if (i < Controls.Count)
                    {
                        Program.WalletXiropht.ListControlSizeCreateWallet.Add(
                            new Tuple<Size, Point>(Controls[i].Size, Controls[i].Location));
                    }
                }
            }
        }

        private void SaveWalletFilePath()
        {
            var saveFileDialogWallet = new SaveFileDialog
            {
                InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory,
                Filter = @"Wallet File (*.xir) | *.xir",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (saveFileDialogWallet.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialogWallet.FileName != "")
                {
                    textBoxPathWallet.Text = saveFileDialogWallet.FileName;
                }
            }
        }

        private void ButtonCreateYourWallet_Click(object sender, EventArgs e)
        {
            CreateWalletAsync();
        }

        private void ButtonSearchNewWalletFile_Click(object sender, EventArgs e)
        {
            SaveWalletFilePath();
        }

        private void CreateWallet_Load(object sender, EventArgs e)
        {
            UpdateStyles();
            Program.WalletXiropht.ResizeWalletInterface();
        }

        private void CreateWallet_Resize(object sender, EventArgs e)
        {
            UpdateStyles();
        }

        private async void CreateWalletAsync()
        {
            if (InCreation)
            {
                Program.WalletXiropht.ClassWalletObject.FullDisconnection(true);
                InCreation = false;
            }

            if (CheckPasswordValidity())
            {
                if (textBoxPathWallet.Text != "")
                {
                    if (textBoxSelectWalletPassword.Text != "")
                    {
                        if (Program.WalletXiropht.ClassWalletObject != null)
                        {
                            Program.WalletXiropht.InitializationWalletObject();
                        }
                        if (await Program.WalletXiropht.ClassWalletObject.InitializationWalletConnection("", textBoxSelectWalletPassword.Text, "",
                                ClassWalletPhase.Create))
                        {
                            Program.WalletXiropht.ClassWalletObject.WalletNewPassword = textBoxSelectWalletPassword.Text;
                            Program.WalletXiropht.ClassWalletObject.ListenSeedNodeNetworkForWallet();

                            InCreation = true;
                            Program.WalletXiropht.ClassWalletObject.WalletDataCreationPath = textBoxPathWallet.Text;

                            await Task.Factory.StartNew(async delegate ()
                            {
                                if (await Program.WalletXiropht.ClassWalletObject.WalletConnect.SendPacketWallet(Program.WalletXiropht.ClassWalletObject.Certificate, string.Empty, false))
                                {
                                    await Task.Delay(100);
                                    if (!await Program.WalletXiropht.ClassWalletObject
                                        .SendPacketWalletToSeedNodeNetwork(
                                            ClassWalletCommand.ClassWalletSendEnumeration.CreatePhase + "|" +
                                            textBoxSelectWalletPassword.Text))
                                    {
#if WINDOWS
                                    ClassFormPhase.MessageBoxInterface(
                                            ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_ERROR_CANT_CONNECT_MESSAGE_CONTENT_TEXT"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                                   MethodInvoker invoke = () => MessageBox.Show(Program.WalletXiropht,
                                        ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_ERROR_CANT_CONNECT_MESSAGE_CONTENT_TEXT"));
                                    Program.WalletXiropht.BeginInvoke(invoke);
#endif
                                }

                                    void MethodInvoker() => textBoxSelectWalletPassword.Text = "";
                                    BeginInvoke((MethodInvoker)MethodInvoker);
                                    CheckPasswordValidity();

                                }
                                else
                                {
#if WINDOWS
                                ClassFormPhase.MessageBoxInterface(
                                       ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_ERROR_CANT_CONNECT_MESSAGE_CONTENT_TEXT"), string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                                MethodInvoker invoke = () => MessageBox.Show(Program.WalletXiropht,
                                    ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_ERROR_CANT_CONNECT_MESSAGE_CONTENT_TEXT"));
                                Program.WalletXiropht.BeginInvoke(invoke);
#endif
                            }
                            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Current);
                        }
                    }
                }
            }
            else
            {
#if WINDOWS
                ClassFormPhase.MessageBoxInterface(
                    ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_LABEL_PASSWORD_REQUIREMENT_TEXT"),
                    ClassTranslation.GetLanguageTextFromOrder("WALLET_NETWORK_OBJECT_CREATE_WALLET_PASSWORD_ERROR2_TITLE_TEXT"), MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                        await Task.Factory.StartNew(() =>
                        {
                            MethodInvoker invoke = () => MessageBox.Show(Program.WalletXiropht,
                                ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_LABEL_PASSWORD_REQUIREMENT_TEXT"),
                                ClassTranslation.GetLanguageTextFromOrder("WALLET_NETWORK_OBJECT_CREATE_WALLET_PASSWORD_ERROR2_TITLE_TEXT"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Program.WalletXiropht.BeginInvoke(invoke);
                        }).ConfigureAwait(false);

#endif
            }
        }

        private void textBoxSelectWalletPassword_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CreateWalletAsync();
            }
        }

        /// <summary>
        /// Check password validity
        /// </summary>
        private bool CheckPasswordValidity()
        {
            if (textBoxSelectWalletPassword.Text.Length >= ClassConnectorSetting.WalletMinPasswordLength)
            {
                if (ClassUtility.CheckPassword(textBoxSelectWalletPassword.Text))
                {
                    MethodInvoker invoke = () => pictureBoxPasswordStatus.BackgroundImage = Properties.Resources.valid;
                    BeginInvoke(invoke);
                    return true;
                }
                else
                {
                    MethodInvoker invoke = () => pictureBoxPasswordStatus.BackgroundImage = Properties.Resources.error;
                    BeginInvoke(invoke);
                }
            }
            else
            {
                MethodInvoker invoke = () => pictureBoxPasswordStatus.BackgroundImage = Properties.Resources.error;
                BeginInvoke(invoke);

            }
            return false;
        }

        private void textBoxSelectWalletPassword_TextChanged(object sender, EventArgs e)
        {
            CheckPasswordValidity();
        }

    }
}