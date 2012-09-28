namespace Helper.Input
{
    partial class FlowLayoutTabPage
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
            this.flpBindings = new System.Windows.Forms.FlowLayoutPanel();

            this.SuspendLayout();
            // 
            // flpBindings
            // 
            this.flpBindings.AutoScroll = true;
            this.flpBindings.Location = new System.Drawing.Point(209, 264);
            this.flpBindings.Name = "flpBindings";
            this.flpBindings.Size = new System.Drawing.Size(149, 123);
            this.flpBindings.TabIndex = 0;
            this.flpBindings.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // FlowLayoutTabPage
            //             
            this.Name = "FlowLayoutTabPage";
            this.Size = new System.Drawing.Size(185, 170);
            this.Controls.Add(flpBindings);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flpBindings;
    }
}
