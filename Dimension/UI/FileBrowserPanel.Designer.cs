namespace Dimension.UI
{
    partial class FileBrowserPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.foldersView = new System.Windows.Forms.TreeView();
            this.filesView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.historyBox = new System.Windows.Forms.TextBox();
            this.inputBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.foldersView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.filesView);
            this.splitContainer1.Size = new System.Drawing.Size(700, 453);
            this.splitContainer1.SplitterDistance = 227;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 1;
            // 
            // foldersView
            // 
            this.foldersView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.foldersView.Location = new System.Drawing.Point(0, 0);
            this.foldersView.Margin = new System.Windows.Forms.Padding(2);
            this.foldersView.Name = "foldersView";
            this.foldersView.Size = new System.Drawing.Size(227, 453);
            this.foldersView.TabIndex = 0;
            this.foldersView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.foldersView_AfterSelect);
            // 
            // filesView
            // 
            this.filesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.filesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filesView.FullRowSelect = true;
            this.filesView.Location = new System.Drawing.Point(0, 0);
            this.filesView.Margin = new System.Windows.Forms.Padding(2);
            this.filesView.Name = "filesView";
            this.filesView.Size = new System.Drawing.Size(470, 453);
            this.filesView.TabIndex = 0;
            this.filesView.UseCompatibleStateImageBehavior = false;
            this.filesView.View = System.Windows.Forms.View.Details;
            this.filesView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.filesView_MouseDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Filename";
            this.columnHeader1.Width = 265;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.Width = 104;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(714, 485);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(706, 459);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Files";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.historyBox);
            this.tabPage2.Controls.Add(this.inputBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(706, 459);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Chat";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // historyBox
            // 
            this.historyBox.BackColor = System.Drawing.SystemColors.Window;
            this.historyBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyBox.Location = new System.Drawing.Point(3, 3);
            this.historyBox.Margin = new System.Windows.Forms.Padding(2);
            this.historyBox.Multiline = true;
            this.historyBox.Name = "historyBox";
            this.historyBox.ReadOnly = true;
            this.historyBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.historyBox.Size = new System.Drawing.Size(700, 431);
            this.historyBox.TabIndex = 5;
            // 
            // inputBox
            // 
            this.inputBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputBox.Location = new System.Drawing.Point(3, 434);
            this.inputBox.Margin = new System.Windows.Forms.Padding(2);
            this.inputBox.Multiline = true;
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(700, 22);
            this.inputBox.TabIndex = 6;
            this.inputBox.TextChanged += new System.EventHandler(this.inputBox_TextChanged);
            this.inputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputBox_KeyDown);
            // 
            // FileBrowserPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FileBrowserPanel";
            this.Size = new System.Drawing.Size(714, 485);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView foldersView;
        private System.Windows.Forms.ListView filesView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox historyBox;
        private System.Windows.Forms.TextBox inputBox;
    }
}
