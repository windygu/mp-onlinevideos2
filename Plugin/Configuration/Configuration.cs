using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;

namespace OnlineVideos
{
	/// <summary>
	/// Description of Configuration.
	/// </summary>
	public partial class Configuration : Form
	{
		public Configuration()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

            propertyGridUserConfig.BrowsableAttributes = new AttributeCollection(new CategoryAttribute("OnlineVideosUserConfiguration"));
		}

		public void Configuration_Load(object sender, EventArgs e)
        {
            /** fill "Codecs" tab **/
            SetInfosFromCodecs();

            /** fill "General" tab **/
            OnlineVideoSettings settings = OnlineVideoSettings.getInstance();
            lblVersion.Text = "Version: " + new System.Reflection.AssemblyName(System.Reflection.Assembly.GetExecutingAssembly().FullName).Version.ToString();            
            tbxScreenName.Text = settings.BasicHomeScreenName;
			txtThumbLoc.Text = settings.msThumbLocation;
            txtDownloadDir.Text = settings.msDownloadDir;
            txtFilters.Text = settings.msFilterArray != null ? string.Join(",", settings.msFilterArray) : "";
            chkUseAgeConfirmation.Checked = settings.useAgeConfirmation;            
            tbxPin.Text = settings.pinAgeConfirmation;
            tbxWebCacheTimeout.Text = settings.cacheTimeout.ToString();
            tbxUtilTimeout.Text = settings.utilTimeout.ToString();
            tbxWMPBuffer.Text = settings.wmpbuffer.ToString();

            /** fill "Sites" tab **/
            // utils combobox
            foreach (string site in SiteUtilFactory.GetAllNames()) cbSiteUtil.Items.Add(site);
            // language identifiers combobox
            List<string> cultureNames = new List<string>();            
            foreach (System.Globalization.CultureInfo ci in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures))
            {
                string name = ci.Name.IndexOf('-') >= 0 ? ci.Name.Substring(0, ci.Name.IndexOf('-')) : ci.Name;
                if (!cultureNames.Contains(name)) cultureNames.Add(name);
            }
            cultureNames.Sort();
            cbLanguages.Items.AddRange(cultureNames.ToArray());            

            // set bindings            
            bindingSourceSiteSettings.DataSource = OnlineVideoSettings.getInstance().SiteSettingsList;
		}
		
		void SiteListSelectedValueChanged(object sender, EventArgs e)
        {            
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            BindingList<RssLink> rssLinks = new BindingList<RssLink>();

            tvGroups_AfterSelect(tvGroups, new TreeViewEventArgs(null, TreeViewAction.Unknown));
            tvGroups.Nodes.Clear();

            iconSite.Image = null;

            if (site != null)
            {                                                
                foreach (Category aCat in site.Categories)
                {
                    if (aCat is RssLink) rssLinks.Add(aCat as RssLink);
                    else if (aCat is Group)
                    {
                        TreeNode aGroupNode = new TreeNode(aCat.Name);
                        aGroupNode.Tag = aCat;
                        foreach (Channel aChannel in (aCat as Group).Channels)
                        {
                            TreeNode node = new TreeNode(aChannel.StreamName);
                            node.Tag = aChannel;
                            aGroupNode.Nodes.Add(node);
                        }
                        tvGroups.Nodes.Add(aGroupNode);
                    }
                }
                tvGroups.ExpandAll();

                Sites.SiteUtilBase siteUtil = SiteUtilFactory.CreateFromShortName(site.UtilName, site);
                propertyGridUserConfig.SelectedObject = siteUtil;

                string image = OnlineVideoSettings.getInstance().BannerIconsDir + @"Icons\" + site.Name + ".png";
                if (System.IO.File.Exists(image)) iconSite.ImageLocation = image;
            }

            bindingSourceRssLink.DataSource = rssLinks;
            RssLinkListSelectedIndexChanged(this, EventArgs.Empty);

            btnAddRss.Enabled = site != null;
            btnAddGroup.Enabled = site != null;

            btnDeleteSite.Enabled = site != null;
            btnReportSite.Enabled = site != null;
            btnPublishSite.Enabled = site != null;
            btnSiteUp.Enabled = site != null;
            btnSiteDown.Enabled = site != null;
		}
		
		void RssLinkListSelectedIndexChanged(object sender, EventArgs e)
		{                        
            // enable/disable all TextBoxes and Buttons for RssLink if one/none is selected
            txtRssUrl.Enabled = RssLinkList.SelectedIndex > -1;
            txtRssName.Enabled = RssLinkList.SelectedIndex > -1;
            txtRssThumb.Enabled = RssLinkList.SelectedIndex > -1;
            btnDeleteRss.Enabled = RssLinkList.SelectedIndex > -1;
		}		
				
		void BtnAddRssClick(object sender, EventArgs e)
		{   
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            RssLink link = new RssLink() { Name = "new", Url = "http://" };
            site.Categories.Add(link);
            ((CurrencyManager)BindingContext[bindingSourceRssLink]).List.Add(link);
            RssLinkList.SelectedIndex = RssLinkList.Items.Count - 1;
            RssLinkListSelectedIndexChanged(this, EventArgs.Empty);
            txtRssName.Focus();
		}
				
		void BtnDeleteRssClick(object sender, EventArgs e)
		{
			if(RssLinkList.SelectedIndex > -1)
            {
                SiteSettings site = siteList.SelectedItem as SiteSettings;
                RssLink link = RssLinkList.SelectedItem as RssLink;
                ((CurrencyManager)BindingContext[bindingSourceRssLink]).RemoveAt(RssLinkList.SelectedIndex);
                site.Categories.Remove(link);
			}			
		}
		
		void ConfigurationFormClosing(object sender, FormClosingEventArgs e)
		{
            DialogResult dr = this.DialogResult;

            if (DialogResult == DialogResult.Cancel)
            {
                dr = MessageBox.Show("If you want to save your changes press Yes.", "Save Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }

            if (dr == DialogResult.OK || dr == DialogResult.Yes)
            {
                OnlineVideoSettings settings = OnlineVideoSettings.getInstance();
                String lsFilter = txtFilters.Text;
                String[] lsFilterArray = lsFilter.Split(new char[] { ',' });
                settings.msFilterArray = lsFilterArray;
                settings.msThumbLocation = txtThumbLoc.Text;
                settings.BasicHomeScreenName = tbxScreenName.Text;                
                settings.msDownloadDir = txtDownloadDir.Text;
                settings.useAgeConfirmation = chkUseAgeConfirmation.Checked;
                settings.pinAgeConfirmation = tbxPin.Text;
                try { settings.cacheTimeout = int.Parse(tbxWebCacheTimeout.Text); } catch { }
                try { settings.utilTimeout = int.Parse(tbxUtilTimeout.Text); } catch { }
                settings.Save();
            }
		}               

        private void chkUseAgeConfirmation_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUseAgeConfirmation.Checked)
                tbxPin.Enabled = true;
            else
            {
                tbxPin.Enabled = false;
                MessageBox.Show("This will allow unprotected access to sexually explicit material. Please ensure that anyone given access to MediaPortal has reached the legal age for viewing such content!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void tvGroups_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                btnSaveChannel.Enabled = true;
                btnDeleteChannel.Enabled = true;
                if (e.Node.Parent == null)
                {
                    tbxChannelName.Enabled = true;
                    tbxChannelName.Text = e.Node.Text;
                    tbxChannelThumb.Enabled = true;
                    tbxChannelThumb.Text = ((Group)e.Node.Tag).Thumb;
                    tbxStreamName.Text = "";
                    tbxStreamName.Enabled = false;
                    tbxStreamUrl.Text = "";
                    tbxStreamUrl.Enabled = false;
                    tbxStreamThumb.Text = "";
                    tbxStreamThumb.Enabled = false;
                    btnAddChannel.Enabled = true;
                }
                else
                {
                    tbxChannelName.Text = "";
                    tbxChannelName.Enabled = false;
                    tbxChannelThumb.Text = "";
                    tbxChannelThumb.Enabled = false;
                    tbxStreamName.Text = e.Node.Text;
                    tbxStreamName.Enabled = true;
                    tbxStreamUrl.Text = ((Channel)e.Node.Tag).Url;
                    tbxStreamUrl.Enabled = true;
                    tbxStreamThumb.Text = ((Channel)e.Node.Tag).Thumb;
                    tbxStreamThumb.Enabled = true;
                    btnAddChannel.Enabled = false;
                }
            }
            else
            {
                tbxChannelName.Text = "";
                tbxChannelName.Enabled = false;
                tbxChannelThumb.Text = "";
                tbxChannelThumb.Enabled = false;
                tbxStreamName.Text = "";
                tbxStreamName.Enabled = false;
                tbxStreamUrl.Text = "";
                tbxStreamUrl.Enabled = false;
                tbxStreamThumb.Text = "";
                tbxStreamThumb.Enabled = false;
                btnSaveChannel.Enabled = false;
                btnDeleteChannel.Enabled = false;
                btnAddChannel.Enabled = false;
            }
        }

        private void btnSaveChannel_Click(object sender, EventArgs e)
        {
            if (tvGroups.SelectedNode != null)
            {
                if (tvGroups.SelectedNode.Parent == null)
                {
                    SiteSettings site = siteList.SelectedItem as SiteSettings;
                    Group group = tvGroups.SelectedNode.Tag as Group;
                    site.Categories.Remove(group);
                    group.Name = tbxChannelName.Text;
                    group.Thumb = tbxChannelThumb.Text != "" ? tbxChannelThumb.Text : null;
                    tvGroups.SelectedNode.Text = tbxChannelName.Text;
                    site.Categories.Add(group);                    
                }
                else
                {
                    Channel channel = tvGroups.SelectedNode.Tag as Channel;
                    channel.StreamName = tbxStreamName.Text;
                    tvGroups.SelectedNode.Text = tbxStreamName.Text;
                    channel.Url = tbxStreamUrl.Text;
                    channel.Thumb = tbxStreamThumb.Text != "" ? tbxStreamThumb.Text : null;
                }
            }
        }

        private void btnDeleteChannel_Click(object sender, EventArgs e)
        {
            if (tvGroups.SelectedNode != null)
            {
                if (tvGroups.SelectedNode.Parent == null)
                {
                    Group group = tvGroups.SelectedNode.Tag as Group;
                    SiteSettings site = siteList.SelectedItem as SiteSettings;
                    site.Categories.Remove(group);
                    tvGroups.Nodes.Remove(tvGroups.SelectedNode);
                }
                else
                {
                    Channel channel = tvGroups.SelectedNode.Tag as Channel;
                    Group group = tvGroups.SelectedNode.Parent.Tag as Group;
                    group.Channels.Remove(channel);
                    tvGroups.Nodes.Remove(tvGroups.SelectedNode);
                }
                tvGroups.SelectedNode = null;
                tvGroups_AfterSelect(tvGroups, new TreeViewEventArgs(null));
            }
        }

        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            Group g = new Group();
            g.Name = "New";
            site.Categories.Add(g);
            TreeNode node = new TreeNode("New");
            node.Tag = g;
            tvGroups.Nodes.Add(node);
            tvGroups.SelectedNode = node;
            tbxChannelName.Focus();
        }

        private void btnAddChannel_Click(object sender, EventArgs e)
        {
            Group group = tvGroups.SelectedNode.Tag as Group;
            Channel c = new Channel();
            c.StreamName = "New";
            if (group.Channels == null) group.Channels = new BindingList<Channel>();
            group.Channels.Add(c);
            TreeNode node = new TreeNode("New");
            node.Tag = c;
            tvGroups.SelectedNode.Nodes.Add(node);
            tvGroups.SelectedNode = node;
            tbxStreamName.Focus();
        }

        private void btnBrowseForDlFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtDownloadDir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        void SetInfosFromCodecs()
        {
            CodecConfiguration cc = OnlineVideoSettings.getInstance().CodecConfiguration;

            chkFLVSplitterInstalled.Checked = cc.MPC_HC_FLVSplitter.IsInstalled;
            tbxFLVSplitter.Text = cc.MPC_HC_FLVSplitter.IsInstalled  ? string.Format("{0} | {1}", cc.MPC_HC_FLVSplitter.CodecFile, cc.MPC_HC_FLVSplitter.Version) : "";

            chkMP4SplitterInstalled.Checked = cc.MPC_HC_MP4Splitter.IsInstalled;
            tbxMP4Splitter.Text = cc.MPC_HC_MP4Splitter.IsInstalled ? string.Format("{0} | {1}", cc.MPC_HC_MP4Splitter.CodecFile, cc.MPC_HC_MP4Splitter.Version) : "";

            if (!chkMP4SplitterInstalled.Checked)
            {
                chkMP4SplitterInstalled.Checked = cc.HaaliMediaSplitter.IsInstalled;
                tbxMP4Splitter.Text = cc.HaaliMediaSplitter.IsInstalled ? string.Format("{0} | {1}", cc.HaaliMediaSplitter.CodecFile, cc.HaaliMediaSplitter.Version) : "";
            }

            chkWMVSplitterInstalled.Checked = cc.WM_ASFReader.IsInstalled;
            tbxWMVSplitter.Text = cc.WM_ASFReader.IsInstalled ? string.Format("{0} | {1}", cc.WM_ASFReader.CodecFile, cc.WM_ASFReader.Version) : "";

            chkAVISplitterInstalled.Checked = cc.AVI_Splitter.IsInstalled;
            tbxAVISplitter.Text = cc.AVI_Splitter.IsInstalled ? string.Format("{0} | {1}", cc.AVI_Splitter.CodecFile, cc.AVI_Splitter.Version) : "";
        }

        private void btnAddSite_Click(object sender, EventArgs e)
        {
            OnlineVideoSettings settings = OnlineVideoSettings.getInstance();
            SiteSettings site = new SiteSettings();
            site.Name = "New";
            site.UtilName = "GenericSite";
            site.IsEnabled = true;
            bindingSourceSiteSettings.Add(site);
            siteList.SelectedItem = site;
            txtSiteName.Focus();
        }

        private void btnDeleteSite_Click(object sender, EventArgs e)
        {
            OnlineVideoSettings settings = OnlineVideoSettings.getInstance();
            SiteSettings site = siteList.SelectedItem as SiteSettings;            
            bindingSourceSiteSettings.Remove(site);            
        }

        private void btnSiteUp_Click(object sender, EventArgs e)
        {
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            siteList.SelectedIndex = -1;
            bindingSourceSiteSettings.SuspendBinding();

            int currentPos = OnlineVideoSettings.getInstance().SiteSettingsList.IndexOf(site);
            OnlineVideoSettings.getInstance().SiteSettingsList.Remove(site);
            if (currentPos == 0) OnlineVideoSettings.getInstance().SiteSettingsList.Add(site);
            else OnlineVideoSettings.getInstance().SiteSettingsList.Insert(currentPos - 1, site);

            bindingSourceSiteSettings.ResumeBinding();
            bindingSourceSiteSettings.Position = OnlineVideoSettings.getInstance().SiteSettingsList.IndexOf(site); 
            bindingSourceSiteSettings.ResetCurrentItem();
        }

        private void btnSiteDown_Click(object sender, EventArgs e)
        {
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            siteList.SelectedIndex = -1;
            bindingSourceSiteSettings.SuspendBinding();

            int currentPos = OnlineVideoSettings.getInstance().SiteSettingsList.IndexOf(site);
            OnlineVideoSettings.getInstance().SiteSettingsList.Remove(site);
            if (currentPos >= OnlineVideoSettings.getInstance().SiteSettingsList.Count) OnlineVideoSettings.getInstance().SiteSettingsList.Insert(0, site);
            else OnlineVideoSettings.getInstance().SiteSettingsList.Insert(currentPos + 1, site);

            bindingSourceSiteSettings.ResumeBinding();
            bindingSourceSiteSettings.Position = OnlineVideoSettings.getInstance().SiteSettingsList.IndexOf(site);
            bindingSourceSiteSettings.ResetCurrentItem();
        }

        private void btnImportSite_Click(object sender, EventArgs e)
        {
            try
            {
                ImExportXml dialog = new ImExportXml();
                if (dialog.ShowDialog() == DialogResult.OK)
                {                    
                    string xml = dialog.txtXml.Text;
                    xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<OnlineVideoSites xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
<Sites>
" + xml + @"
</Sites>
</OnlineVideoSites>";
                    System.IO.StringReader sr = new System.IO.StringReader(xml);
                    System.Xml.Serialization.XmlSerializer ser = OnlineVideoSettings.getInstance().XmlSerImp.GetSerializer(typeof(SerializableSettings));
                    SerializableSettings s = (SerializableSettings)ser.Deserialize(sr);
                    if (s.Sites != null)
                    {
                        foreach (SiteSettings site in s.Sites) OnlineVideoSettings.getInstance().SiteSettingsList.Add(site);
                        if (s.Sites.Count > 0) siteList.SelectedItem = s.Sites[s.Sites.Count - 1];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }           
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            SiteSettings siteSettings = (SiteSettings)bindingSourceSiteSettings.Current;
            Sites.SiteUtilBase siteUtil = SiteUtilFactory.CreateFromShortName(siteSettings.UtilName, siteSettings);

            ConfigurationAdvanced ca = new ConfigurationAdvanced();
            ca.Text += " - " + siteSettings.UtilName;
            ca.propertyGrid.SelectedObject = siteUtil;
            if (ca.ShowDialog() == DialogResult.OK)
            {
                // find and set all configuration fields that are not default

                // 1. build a list of all the Fields that are used for OnlineVideosConfiguration
                List<FieldInfo> fieldInfos = new List<FieldInfo>();
                foreach (FieldInfo field in siteUtil.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object[] attrs = field.GetCustomAttributes(typeof(CategoryAttribute), false);
                    if (attrs.Length > 0 && ((CategoryAttribute)attrs[0]).Category == "OnlineVideosConfiguration")
                    {
                        fieldInfos.Add(field);
                    }
                }

                // 2. get a "clean" site by creating it with empty SiteSettings
                siteSettings.Configuration = new StringHash();
                Sites.SiteUtilBase cleanSiteUtil = SiteUtilFactory.CreateFromShortName(siteSettings.UtilName, siteSettings);

                // 3. compare and collect different settings
                foreach (FieldInfo field in fieldInfos)
                {
                    object defaultValue = field.GetValue(cleanSiteUtil);
                    object newValue = field.GetValue(siteUtil);
                    if (defaultValue != newValue)
                    {
                        siteSettings.Configuration.Add(field.Name, newValue.ToString());
                    }
                }
            }
        }
        
        private void CheckValidNumber(object sender, CancelEventArgs e)
        {
            string error = null;
            uint value = 0;
            if (!uint.TryParse((sender as TextBox).Text, out value))
            {
                error = (sender as TextBox).Text + " is not a valid number!";
                e.Cancel = true;
            }
            errorProvider1.SetError(sender as TextBox, error);
        }

        private void btnPublishSite_Click(object sender, EventArgs e)
        {
            OnlineVideoSettings settings = OnlineVideoSettings.getInstance();
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            if (string.IsNullOrEmpty(settings.email) || string.IsNullOrEmpty(settings.password))
            {
                if (MessageBox.Show("Do you want to register an Email now?", "Registration required!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    RegisterEmail reFrm = new RegisterEmail();
                    reFrm.tbxEmail.Text = settings.email;
                    reFrm.tbxPassword.Text = settings.password;
                    if (reFrm.ShowDialog() == DialogResult.OK)
                    {
                        settings.email = reFrm.tbxEmail.Text;
                        settings.password = reFrm.tbxPassword.Text;
                    }
                }
                return;
            }
            // set current Time to last updated in the xml, so it can be compared later
            DateTime lastUdpBkp = site.LastUpdated;
            site.LastUpdated = DateTime.Now;
            System.Xml.Serialization.XmlSerializer ser = OnlineVideoSettings.getInstance().XmlSerImp.GetSerializer(typeof(SerializableSettings));
            SerializableSettings s = new SerializableSettings() { Sites = new BindingList<SiteSettings>() };
            s.Sites.Add(site);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ser.Serialize(ms, s);
            ms.Position = 0;
            System.Xml.XmlDocument siteDoc = new System.Xml.XmlDocument();
            siteDoc.Load(ms);
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Encoding = System.Text.Encoding.UTF8;
            xmlSettings.Indent = true;
            xmlSettings.OmitXmlDeclaration = true;
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, xmlSettings);
            siteDoc.SelectSingleNode("//Site").WriteTo(writer);
            writer.Flush();
            string siteXmlString = sb.ToString();
            byte[] icon = null;
            if (System.IO.File.Exists(OnlineVideoSettings.getInstance().BannerIconsDir + @"Icons\" + site.Name + ".png"))
                icon = System.IO.File.ReadAllBytes(OnlineVideoSettings.getInstance().BannerIconsDir + @"Icons\" + site.Name + ".png");
            byte[] banner = null;
            if (System.IO.File.Exists(OnlineVideoSettings.getInstance().BannerIconsDir + @"Banners\" + site.Name + ".png"))
                banner = System.IO.File.ReadAllBytes(OnlineVideoSettings.getInstance().BannerIconsDir + @"Banners\" + site.Name + ".png");
            bool success = false;
            try
            {
                OnlineVideosWebservice.OnlineVideosService ws = new OnlineVideos.OnlineVideosWebservice.OnlineVideosService();
                string msg = "";
                success = ws.SubmitSite(settings.email, settings.password, siteXmlString, icon, banner, out msg);
                MessageBox.Show(msg, success ? "Success" : "Error", MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // if the site was not submitted, restore old last updated date, so saving won't write the wrong value
            if (!success) site.LastUpdated = lastUdpBkp;
        }

        private void btnReportSite_Click(object sender, EventArgs e)
        {
            SiteSettings site = siteList.SelectedItem as SiteSettings;
            SubmitSiteReport ssrFrm = new SubmitSiteReport() { SiteName = site.Name };
            ssrFrm.ShowDialog();
        }
	}
}
