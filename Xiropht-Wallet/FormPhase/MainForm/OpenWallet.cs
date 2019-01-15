﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;
using Xiropht_Connector_All.Utils;
using Xiropht_Connector_All.Wallet;
using Xiropht_Wallet.Wallet;

namespace Xiropht_Wallet.FormPhase.MainForm
{
    public partial class OpenWallet : Form
    {
        private string _walletFileData;
        public string _fileSelectedPath;

        public OpenWallet()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        private void ButtonSearchWalletFile_Click(object sender, EventArgs e)
        {
            var openWalletFile = new OpenFileDialog
            {
                InitialDirectory = Directory.GetCurrentDirectory(),
                Filter = "Xiropht Wallet (*.xir) | *.xir",
                FilterIndex = 2,
                RestoreDirectory = true
            };
            if (openWalletFile.ShowDialog() == DialogResult.OK)
            {
                var threadReadWalletFileData = new Thread(delegate()
                {

                    _fileSelectedPath = openWalletFile.FileName;
                    labelOpenFileSelected.BeginInvoke((MethodInvoker) delegate()
                    {
                        labelOpenFileSelected.Text = ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_LABEL_FILE_SELECTED_TEXT") + " " + openWalletFile.FileName;
                    });
                    try
                    {
                        var streamReaderWalletFile = new StreamReader(openWalletFile.FileName);
                        _walletFileData = streamReaderWalletFile.ReadToEnd();
                        streamReaderWalletFile.Close();
                        ClassWalletObject.WalletLastPathFile = openWalletFile.FileName;
                    }
                    catch
                    {
                    }
                });
                threadReadWalletFileData.Start();
            }
        }

        private async void ButtonOpenYourWallet_Click(object sender, EventArgs e)
        {
            await OpenAndConnectWallet();
        }
        
        /// <summary>
        /// Open and connect the wallet.
        /// </summary>
        /// <returns></returns>
        private async Task OpenAndConnectWallet()
        {
            if (textBoxPasswordWallet.Text == "")
            {
                MetroMessageBox.Show(ClassFormPhase.WalletXiropht, ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_NO_PASSWORD_WRITTED_CONTENT_TEXT"),
                    ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_NO_PASSWORD_WRITTED_TITLE_TEXT"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool error = false;

            string passwordEncrypted = ClassAlgo.GetEncryptedResult(ClassAlgoEnumeration.Rijndael,
                textBoxPasswordWallet.Text, textBoxPasswordWallet.Text, ClassWalletNetworkSetting.KeySize);

            ClassWalletObject.WalletDataDecrypted = ClassAlgo.GetDecryptedResult(ClassAlgoEnumeration.Rijndael,
                _walletFileData, passwordEncrypted, ClassWalletNetworkSetting.KeySize); // AES
            if (ClassWalletObject.WalletDataDecrypted == "WRONG")
            {
                error = true;
            }

            if (error)
            {
                ClassWalletObject.WalletDataDecrypted = ClassAlgo.GetDecryptedResult(ClassAlgoEnumeration.Rijndael,
                    _walletFileData, textBoxPasswordWallet.Text, ClassWalletNetworkSetting.KeySize); // AES
            }

            try
            {
                if (ClassWalletObject.WalletDataDecrypted == "WRONG")
                {
                    MetroMessageBox.Show(ClassFormPhase.WalletXiropht,
                        ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_WRONG_PASSWORD_WRITTED_CONTENT_TEXT"),
                        ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_WRONG_PASSWORD_WRITTED_TITLE_TEXT"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var splitWalletFileDecrypted =
                    ClassWalletObject.WalletDataDecrypted.Split(new[] { "\n" }, StringSplitOptions.None);
                string walletAddress = splitWalletFileDecrypted[0];
                string walletKey = splitWalletFileDecrypted[1];
                if (!await ClassWalletObject.InitializationWalletConnection(walletAddress, textBoxPasswordWallet.Text,
                    walletKey, ClassWalletPhase.Login))
                {
                    textBoxPasswordWallet.Text = "";
                    MetroMessageBox.Show(ClassFormPhase.WalletXiropht,
                        ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_NETWORK_CONTENT_TEXT"), ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_NETWORK_TITLE_TEXT"), MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                textBoxPasswordWallet.Text = "";


                ClassWalletObject.ListenSeedNodeNetworkForWallet();
                if (await ClassWalletObject.WalletConnect.SendPacketWallet(ClassWalletObject.Certificate, string.Empty, false))
                {
                    await Task.Delay(1000);
                    await ClassWalletObject.WalletConnect.SendPacketWallet(
                        "WALLET|" + ClassWalletObject.WalletConnect.WalletAddress, ClassWalletObject.Certificate, true);
                    _walletFileData = string.Empty;
                    _fileSelectedPath = string.Empty;
                    labelOpenFileSelected.Text = ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_LABEL_FILE_SELECTED_TEXT");

                }
            }
            catch
            {
                MetroMessageBox.Show(ClassFormPhase.WalletXiropht,
                    ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_NETWORK_WRONG_PASSWORD_WRITTED_CONTENT_TEXT"),
                    ClassTranslation.GetLanguageTextFromOrder("OPEN_WALLET_ERROR_MESSAGE_NETWORK_WRONG_PASSWORD_WRITTED_TITLE_TEXT"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Get each control of the interface.
        /// </summary>
        public void GetListControl()
        {
            if (ClassFormPhase.WalletXiropht.ListControlSizeOpenWallet.Count == 0)
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    if (i < Controls.Count)
                    {
                        ClassFormPhase.WalletXiropht.ListControlSizeOpenWallet.Add(
                            new Tuple<Size, Point>(Controls[i].Size, Controls[i].Location));
                    }
                }
            }
        }

        private void OpenWallet_Load(object sender, EventArgs e)
        {
            UpdateStyles();
            ClassFormPhase.WalletXiropht.ResizeWalletInterface();
        }

        private void OpenWallet_Resize(object sender, EventArgs e)
        {
            UpdateStyles();
        }

        private async void textBoxPasswordWallet_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Open wallet on press enter key.
            {
                await OpenAndConnectWallet();
            }
        }
    }
}