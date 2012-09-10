namespace Input
{
    partial class KeyBindingControl
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
            this.lblAlias = new System.Windows.Forms.Label();
            this.tbBinding = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblAlias
            // 
            this.lblAlias.AutoSize = true;
            this.lblAlias.Location = new System.Drawing.Point(3, 9);
            this.lblAlias.Name = "lblAlias";
            this.lblAlias.Size = new System.Drawing.Size(29, 13);
            this.lblAlias.TabIndex = 0;
            this.lblAlias.Text = "Alias";
            // 
            // tbBinding
            // 
            this.tbBinding.Location = new System.Drawing.Point(191, 6);
            this.tbBinding.Name = "tbBinding";
            this.tbBinding.ReadOnly = true;
            this.tbBinding.Size = new System.Drawing.Size(122, 20);
            this.tbBinding.TabIndex = 1;
            this.tbBinding.Click += new System.EventHandler(this.tbBinding_Click);
            // 
            // KeyBindingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbBinding);
            this.Controls.Add(this.lblAlias);
            this.Name = "KeyBindingControl";
            this.Size = new System.Drawing.Size(316, 32);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAlias;
        private System.Windows.Forms.TextBox tbBinding;
    }
}
