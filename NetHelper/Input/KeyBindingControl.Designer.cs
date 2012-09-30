namespace Helper.Input
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
            this.txtBinding = new System.Windows.Forms.TextBox();
            this.lblEvent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblAlias
            // 
            this.lblAlias.AutoSize = true;
            this.lblAlias.Location = new System.Drawing.Point(3, 5);
            this.lblAlias.Name = "lblAlias";
            this.lblAlias.Size = new System.Drawing.Size(29, 13);
            this.lblAlias.TabIndex = 0;
            this.lblAlias.Text = "Alias";
            // 
            // txtBinding
            // 
            this.txtBinding.Location = new System.Drawing.Point(167, 0);
            this.txtBinding.Name = "txtBinding";
            this.txtBinding.ReadOnly = true;
            this.txtBinding.Size = new System.Drawing.Size(146, 20);
            this.txtBinding.TabIndex = 1;
            this.txtBinding.Click += new System.EventHandler(this.tbBinding_Click);
            // 
            // lblEvent
            // 
            this.lblEvent.AutoSize = true;
            this.lblEvent.Location = new System.Drawing.Point(319, 5);
            this.lblEvent.Name = "lblEvent";
            this.lblEvent.Size = new System.Drawing.Size(35, 13);
            this.lblEvent.TabIndex = 2;
            this.lblEvent.Text = "Event";
            // 
            // KeyBindingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.Controls.Add(this.lblEvent);
            this.Controls.Add(this.txtBinding);
            this.Controls.Add(this.lblAlias);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "KeyBindingControl";
            this.Size = new System.Drawing.Size(438, 21);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAlias;
        private System.Windows.Forms.TextBox txtBinding;
        private System.Windows.Forms.Label lblEvent;
    }
}
