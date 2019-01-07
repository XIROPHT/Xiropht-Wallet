﻿using System;
using System.Windows.Forms;
using MetroFramework;

namespace Xiropht_Wallet.FormPhase.ParallelForm
{
    public partial class CreateWalletSuccessForm : Form
    {
        public string PrivateKey;
        public string PublicKey;
        public string PinCode;

        public CreateWalletSuccessForm()
        {
            InitializeComponent();
        }

        private void ButtonAcceptAndCloseWalletInformation_Click(object sender, EventArgs e)
        {
            if (MetroMessageBox.Show(ClassFormPhase.WalletXiropht,
                    ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_ACCEPT_WALLET_INFORMATION_MESSAGE_CONTENT_TEXT"),
                    ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_ACCEPT_WALLET_INFORMATION_MESSAGE_TITLE_TEXT"), MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                labelYourPrivateKey.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_PRIVATE_KEY_TEXT");
                labelYourPublicKey.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_PUBLIC_KEY_TEXT");
                labelYourPinCode.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_PIN_CODE_TEXT");
                PrivateKey = string.Empty;
                PublicKey = string.Empty;
                PinCode = string.Empty;
                Close();
            }
            else
            {
                MetroMessageBox.Show(ClassFormPhase.WalletXiropht,
                    ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_ACCEPT_WALLET_INFORMATION_MESSAGE_SAFE_CONTENT_TEXT"));
            }
        }

        private void ButtonCopyWalletInformation_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(labelYourPublicKey.Text + " " + Environment.NewLine +
                             labelYourPrivateKey.Text + " " + Environment.NewLine +
                             labelYourPinCode.Text);
            MetroMessageBox.Show(ClassFormPhase.WalletXiropht,
                ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_COPY_WALLET_INFORMATION_CONTENT_TEXT"),
                ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_COPY_WALLET_INFORMATION_TITLE_TEXT"), MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void CreateWalletSuccessForm_Load(object sender, EventArgs e)
        {
            labelCreateWalletInformation.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_INFORMATION_TEXT");
            labelYourPinCode.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_PIN_CODE_TEXT") + " "+PinCode;
            labelYourPrivateKey.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_PRIVATE_KEY_TEXT") + " "+PrivateKey;
            labelYourPublicKey.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_LABEL_PUBLIC_KEY_TEXT") + " "+PublicKey;
            buttonAcceptAndCloseWalletInformation.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_ACCEPT_WALLET_INFORMATION_TEXT");
            buttonCopyWalletInformation.Text = ClassTranslation.GetLanguageTextFromOrder("CREATE_WALLET_SUBMENU_BUTTON_COPY_WALLET_INFORMATION_TEXT");
        }
    }
}