namespace Dimension.UI
{
    partial class LimitChangeDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.okayButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.typeLabel = new System.Windows.Forms.Label();
            this.noLimitButton = new System.Windows.Forms.RadioButton();
            this.yesLimitButton = new System.Windows.Forms.RadioButton();
            this.valueBox = new System.Windows.Forms.NumericUpDown();
            this.unitComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.valueBox)).BeginInit();
            this.SuspendLayout();
            // 
            // okayButton
            // 
            this.okayButton.Location = new System.Drawing.Point(334, 96);
            this.okayButton.Name = "okayButton";
            this.okayButton.Size = new System.Drawing.Size(75, 23);
            this.okayButton.TabIndex = 0;
            this.okayButton.Text = "Okay";
            this.okayButton.UseVisualStyleBackColor = true;
            this.okayButton.Click += new System.EventHandler(this.okayButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(12, 96);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Location = new System.Drawing.Point(23, 13);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(141, 13);
            this.typeLabel.TabIndex = 2;
            this.typeLabel.Text = "Global Download Rate Limit:";
            // 
            // noLimitButton
            // 
            this.noLimitButton.AutoSize = true;
            this.noLimitButton.Location = new System.Drawing.Point(26, 30);
            this.noLimitButton.Name = "noLimitButton";
            this.noLimitButton.Size = new System.Drawing.Size(63, 17);
            this.noLimitButton.TabIndex = 3;
            this.noLimitButton.TabStop = true;
            this.noLimitButton.Text = "No Limit";
            this.noLimitButton.UseVisualStyleBackColor = true;
            // 
            // yesLimitButton
            // 
            this.yesLimitButton.AutoSize = true;
            this.yesLimitButton.Location = new System.Drawing.Point(26, 53);
            this.yesLimitButton.Name = "yesLimitButton";
            this.yesLimitButton.Size = new System.Drawing.Size(77, 17);
            this.yesLimitButton.TabIndex = 4;
            this.yesLimitButton.TabStop = true;
            this.yesLimitButton.Text = "Fixed Limit:";
            this.yesLimitButton.UseVisualStyleBackColor = true;
            // 
            // valueBox
            // 
            this.valueBox.Location = new System.Drawing.Point(110, 53);
            this.valueBox.Maximum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.valueBox.Name = "valueBox";
            this.valueBox.Size = new System.Drawing.Size(120, 20);
            this.valueBox.TabIndex = 5;
            this.valueBox.ValueChanged += new System.EventHandler(this.valueBox_ValueChanged);
            this.valueBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.valueBox_KeyDown);
            // 
            // unitComboBox
            // 
            this.unitComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.unitComboBox.FormattingEnabled = true;
            this.unitComboBox.Items.AddRange(new object[] {
            "Bytes/Second",
            "Kilobytes/Second",
            "Megabytes/Second",
            "Gigabytes/Second",
            "Terabytes/Second"});
            this.unitComboBox.Location = new System.Drawing.Point(236, 53);
            this.unitComboBox.Name = "unitComboBox";
            this.unitComboBox.Size = new System.Drawing.Size(173, 21);
            this.unitComboBox.TabIndex = 6;
            this.unitComboBox.SelectedIndexChanged += new System.EventHandler(this.unitComboBox_SelectedIndexChanged);
            // 
            // LimitChangeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 131);
            this.Controls.Add(this.unitComboBox);
            this.Controls.Add(this.valueBox);
            this.Controls.Add(this.yesLimitButton);
            this.Controls.Add(this.noLimitButton);
            this.Controls.Add(this.typeLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okayButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LimitChangeDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change Transfer Limit";
            ((System.ComponentModel.ISupportInitialize)(this.valueBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okayButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.RadioButton noLimitButton;
        private System.Windows.Forms.RadioButton yesLimitButton;
        private System.Windows.Forms.NumericUpDown valueBox;
        private System.Windows.Forms.ComboBox unitComboBox;
    }
}