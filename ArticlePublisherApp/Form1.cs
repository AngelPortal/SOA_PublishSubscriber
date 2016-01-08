using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Web.Services2.Addressing;
using Microsoft.Web.Services2.Messaging;

namespace ArticlePublisherApp
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button btnPublish;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtArticle;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.txtArticle = new System.Windows.Forms.TextBox();
			this.btnPublish = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.Location = new System.Drawing.Point(8, 32);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(760, 121);
			this.listBox1.TabIndex = 0;
			// 
			// txtArticle
			// 
			this.txtArticle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtArticle.Location = new System.Drawing.Point(8, 184);
			this.txtArticle.Multiline = true;
			this.txtArticle.Name = "txtArticle";
			this.txtArticle.Size = new System.Drawing.Size(760, 128);
			this.txtArticle.TabIndex = 1;
			this.txtArticle.Text = "";
			// 
			// btnPublish
			// 
			this.btnPublish.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.btnPublish.Location = new System.Drawing.Point(136, 320);
			this.btnPublish.Name = "btnPublish";
			this.btnPublish.Size = new System.Drawing.Size(464, 40);
			this.btnPublish.TabIndex = 2;
			this.btnPublish.Text = "Publish Article";
			this.btnPublish.Click += new System.EventHandler(this.btnPublish_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(240, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Subscribers:";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(8, 160);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(448, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Article/Data To Publish:";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(776, 366);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnPublish);
			this.Controls.Add(this.txtArticle);
			this.Controls.Add(this.listBox1);
			this.Name = "Form1";
			this.Text = "Article Publisher Application";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			Publisher pub = new Publisher();
			pub.NewSubscriberEvent += new NewSubscriberEventHandler(this.ListenForNewSubscribers);
			pub.RemoveSubscriberEvent += new RemoveSubscriberEventHandler(this.ListenForSubscriberRemovals);
			Uri uriThis = new Uri(Literals.LocalhostTCP + "9090/Publisher");
			SoapReceivers.Add(new EndpointReference(uriThis), pub);
		}

		private void ListenForNewSubscribers(string Name, string ID, Uri replyTo)
		{
			this.listBox1.Items.Add(String.Format("Name - {0}\t ID - {1}\t Reply To Uri {2}", Name,  ID,  replyTo.ToString()));
		}

		private void ListenForSubscriberRemovals (string ID)
		{
			foreach (object itm in this.listBox1.Items )
			{
				if (itm.ToString().IndexOf(ID) > 0)
				{
					//remove item
					this.listBox1.Items.Remove(itm);
					return;
				}
			}
		}

		private void btnPublish_Click(object sender, System.EventArgs e)
		{
			if (this.listBox1.Items.Count > 0 )
			{
				System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
				string publishFileName =  ((string)(configurationAppSettings.GetValue("PublishFileName", typeof(string))));
				string publishFolder = ((string)(configurationAppSettings.GetValue("Publish.PublishFolder", typeof(string))));
				StreamWriter sw = null;
				try
				{
					sw = new StreamWriter(publishFolder.Trim() + "\\" + publishFileName.Trim(), false);
					sw.WriteLine(this.txtArticle.Text );
					sw.Close();
				}
				catch ( System.IO.IOException ex)
				{
					MessageBox.Show(ex.ToString());
				}
				finally
				{
					sw.Close();
					sw = null;
				}
			}
		}
	}
}
