using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace PixabayWinForms
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient;
        private int currentPage = 1;
        private string lastQuery = "";
        private List<Panel> selectedImagePanels = new List<Panel>();
        private Button btnDownloadSelected;
        private ProgressBar progressBar;
        private bool _isUpdatingCheckboxes = false;
        private Label lblRateLimit;
        private Label lblRateRemaining;
        private Label lblRateReset;
        private string ApiKey;
        private LinkLabel lblTelegram;

        public Form1()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            InitializeAdditionalControls();
            btnSearch.Click += BtnSearch_Click;
            txtSearch.KeyDown += TxtSearch_KeyDown;
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtSearch.Text = "auto";
            PerformSearch();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PerformSearch();
                e.SuppressKeyPress = true;
            }
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            lastQuery = txtSearch.Text.Trim();
            PerformSearch();
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            currentPage++;
            PerformSearch();
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                PerformSearch();
            }
        }

        private async void PerformSearch()
        {
            try
            {
                SafeInvoke(() => {
                    lblStatus.Text = "Searching...";
                    flowLayoutPanelImages.Controls.Clear();
                    selectedImagePanels.Clear();
                    UpdateDownloadButtonVisibility();
                });

                var urlBuilder = new UriBuilder("https://pixabay.com/api/");
                var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

                query["key"] = ApiKey;

                string searchText = txtSearch.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    query["q"] = searchText;
                }

                query["lang"] = cmbLanguage.SelectedItem?.ToString() ?? "en";
                query["image_type"] = cmbImageType.SelectedItem?.ToString() ?? "photo";
                query["orientation"] = cmbOrientation.SelectedItem?.ToString() ?? "horizontal";
                query["colors"] = cmbColors.SelectedItem.ToString() ?? "black";

                if (cmbCategory.SelectedIndex > 0)
                {
                    query["category"] = cmbCategory.SelectedItem.ToString();
                }

                if (cmbColors.SelectedIndex > 0)
                {
                    query["colors"] = cmbColors.SelectedItem.ToString();
                }

                query["min_width"] = nudMinWidth.Value.ToString();
                query["min_height"] = nudMinHeight.Value.ToString();
                query["safesearch"] = chkSafeSearch.Checked.ToString().ToLower();
                query["order"] = cmbOrder.SelectedItem?.ToString() ?? "latest";
                query["page"] = currentPage.ToString();
                query["per_page"] = nudPerPage.Value.ToString();

                urlBuilder.Query = query.ToString();
                string url = urlBuilder.ToString();

                var responseMessage = await _httpClient.GetAsync(url);
                string json = await responseMessage.Content.ReadAsStringAsync();
                PixabayResponse response = JsonConvert.DeserializeObject<PixabayResponse>(json);

                // Get values from headers
                string limit = responseMessage.Headers.TryGetValues("X-RateLimit-Limit", out var limitValues) ? limitValues.FirstOrDefault() : "-";
                string remaining = responseMessage.Headers.TryGetValues("X-RateLimit-Remaining", out var remainingValues) ? remainingValues.FirstOrDefault() : "-";
                string reset = responseMessage.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues) ? resetValues.FirstOrDefault() : "-";

                // Update labels
                SafeInvoke(() => {
                    lblRateLimit.Text = $"Total limit: {limit}";
                    lblRateRemaining.Text = $"Requests left: {remaining}";
                    lblRateReset.Text = $"Reset in: {reset} seconds";
                    lblTelegram.Text = "Telegram: @master_cryptocom";
                });

                if (response?.Hits != null && response.Hits.Length > 0)
                {
                    SafeInvoke(() =>
                    {
                        lblStatus.Text = $"Found {response.TotalHits} images (page {currentPage}, showing {response.Hits.Length})";
                        lblPageInfo.Text = $"Page {currentPage} of {Math.Ceiling((double)response.TotalHits / (double)nudPerPage.Value)}";
                        progressBar.Maximum = response.Hits.Length;
                        progressBar.Value = 0;
                        progressBar.Visible = true;
                        flowLayoutPanelImages.SuspendLayout();
                    });

                    int i = 0;
                    foreach (var image in response.Hits)
                    {
                        await AddImageToPanel(image);
                        SafeInvoke(() => progressBar.Value = ++i);
                    }

                    SafeInvoke(() =>
                    {
                        flowLayoutPanelImages.ResumeLayout(true);
                        progressBar.Visible = false;
                        btnPrevPage.Enabled = currentPage > 1;
                        btnNextPage.Enabled = response.Hits.Length == (int)nudPerPage.Value;
                    });
                }
                else
                {
                    SafeInvoke(() =>
                    {
                        lblStatus.Text = "No images found. Try changing search parameters.";
                        lblPageInfo.Text = "Page 0 of 0";
                        btnPrevPage.Enabled = false;
                        btnNextPage.Enabled = false;
                    });
                }
            }
            catch (Exception ex)
            {
                SafeInvoke(() => {
                    ShowCopyableError($"Search error: {ex.Message}\r\n\r\nDetails:\r\n{ex}", "Error");
                    lblStatus.Text = "An error occurred during search";
                    progressBar.Visible = false;
                });
            }
        }

        private async Task AddImageToPanel(PixabayImage image)
        {
            Panel imagePanel = new Panel
            {
                Width = 180,
                Height = 220,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            PictureBox pictureBox = new PictureBox
            {
                Width = 170,
                Height = 120,
                Top = 5,
                Left = 5,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Tag = image
            };
            pictureBox.Click += Pic_Click;

            Label lblUser = new Label
            {
                Text = $"Author: {image.User}",
                Top = 130,
                Left = 5,
                Width = 170,
                AutoEllipsis = true
            };

            Label lblTags = new Label
            {
                Text = $"Tags: {image.Tags}",
                Top = 150,
                Left = 5,
                Width = 170,
                Height = 30,
                AutoEllipsis = true
            };

            CheckBox chkSelect = new CheckBox
            {
                Top = 185,
                Left = 5,
                Width = 160,
                Text = "Select",
                Tag = imagePanel
            };
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;

            SafeInvoke(() =>
            {
                imagePanel.Controls.Add(pictureBox);
                imagePanel.Controls.Add(lblUser);
                imagePanel.Controls.Add(lblTags);
                imagePanel.Controls.Add(chkSelect);
                flowLayoutPanelImages.Controls.Add(imagePanel);
            });

            try
            {
                using (var response = await _httpClient.GetAsync(image.PreviewURL))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    Image img = Image.FromStream(memoryStream);
                    SafeInvoke(() => pictureBox.Image = img);
                }
            }
            catch
            {
                SafeInvoke(() => pictureBox.BackColor = Color.LightGray);
            }
        }

        private void Pic_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox pic && pic.Tag is PixabayImage image)
            {
                using (Form detailForm = new Form())
                {
                    detailForm.Text = $"Image by {image.User}";
                    detailForm.Size = new Size(800, 600);
                    detailForm.StartPosition = FormStartPosition.CenterParent;
                    detailForm.MinimizeBox = false;
                    detailForm.MaximizeBox = false;
                    detailForm.FormBorderStyle = FormBorderStyle.FixedDialog;

                    PictureBox largeImage = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom
                    };

                    TableLayoutPanel infoPanel = new TableLayoutPanel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 100,
                        ColumnCount = 1,
                        RowCount = 5,
                        AutoSize = true
                    };

                    infoPanel.Controls.Add(new TextBox
                    {
                        Text = $"Author: {image.User}",
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = SystemColors.Control,
                        TabStop = false,
                        Margin = new Padding(0, 2, 0, 2),
                        Width = 400
                    }, 0, 0);

                    infoPanel.Controls.Add(new TextBox
                    {
                        Text = $"Tags: {image.Tags}",
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = SystemColors.Control,
                        TabStop = false,
                        Width = 400
                    }, 0, 1);

                    infoPanel.Controls.Add(new TextBox
                    {
                        Text = $"Likes: {image.Likes}",
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = SystemColors.Control,
                        TabStop = false,
                        Width = 400
                    }, 0, 2);

                    infoPanel.Controls.Add(new TextBox
                    {
                        Text = $"Size: {image.ImageWidth}x{image.ImageHeight}",
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = SystemColors.Control,
                        TabStop = false,
                        Width = 400
                    }, 0, 3);

                    infoPanel.Controls.Add(new TextBox
                    {
                        Text = $"Views: {image.Views}, Downloads: {image.Downloads}",
                        ReadOnly = true,
                        BorderStyle = BorderStyle.None,
                        BackColor = SystemColors.Control,
                        TabStop = false,
                        Width = 400
                    }, 0, 4);
                    Button btnOpen = new Button
                    {
                        Text = "Open in browser",
                        Dock = DockStyle.Bottom,
                        Height = 30
                    };
                    btnOpen.Click += (s, args) =>
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = image.PageURL,
                            UseShellExecute = true
                        });
                    };

                    Button btnDownload = new Button
                    {
                        Text = "Download image",
                        Dock = DockStyle.Bottom,
                        Height = 30
                    };
                    btnDownload.Click += async (s, args) =>
                    {
                        using (SaveFileDialog saveDialog = new SaveFileDialog())
                        {
                            saveDialog.Filter = "Images|*.jpg;*.jpeg;*.png";
                            saveDialog.Title = "Save image";
                            saveDialog.FileName = $"pixabay_{image.Id}.jpg";

                            if (saveDialog.ShowDialog() == DialogResult.OK)
                            {
                                await DownloadAndSaveImage(image.LargeImageURL, saveDialog.FileName);
                            }
                        }
                    };

                    detailForm.Controls.Add(largeImage);
                    detailForm.Controls.Add(infoPanel);
                    detailForm.Controls.Add(btnOpen);
                    detailForm.Controls.Add(btnDownload);

                    Task.Run(async () =>
                    {
                        try
                        {
                            using (var response = await _httpClient.GetAsync(image.LargeImageURL))
                            using (var stream = await response.Content.ReadAsStreamAsync())
                            using (var memoryStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memoryStream);
                                memoryStream.Position = 0;

                                detailForm.Invoke(new Action(() =>
                                {
                                    largeImage.Image = Image.FromStream(memoryStream);
                                }));
                            }
                        }
                        catch
                        {
                            detailForm.Invoke(new Action(() =>
                            {
                                largeImage.BackColor = Color.LightGray;
                                largeImage.Image = null;
                                MessageBox.Show("Failed to load full-size image", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }));
                        }
                    });

                    detailForm.ShowDialog();
                }
            }
        }


        private void ChkSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCheckboxes)
                return;

            if (sender is CheckBox chk && chk.Tag is Panel panel)
            {
                if (chk.Checked)
                {
                    if (!selectedImagePanels.Contains(panel))
                    {
                        selectedImagePanels.Add(panel);
                        panel.BackColor = Color.LightBlue; // Visual highlight
                    }
                }
                else
                {
                    selectedImagePanels.Remove(panel);
                    panel.BackColor = SystemColors.Control; // Restore default color
                }

                // Update UI
                _isUpdatingCheckboxes = true;
                btnDownloadSelected.Visible = selectedImagePanels.Count > 0;
                UpdateSelectAllCheckboxState();
                _isUpdatingCheckboxes = false;
            }
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            // Ignore programmatic checkbox state changes
            if (_isUpdatingCheckboxes)
                return;

            bool shouldBeChecked = chkSelectAll.Checked;

            try
            {
                _isUpdatingCheckboxes = true;

                foreach (Control control in flowLayoutPanelImages.Controls)
                {
                    if (control is Panel imagePanel)
                    {
                        CheckBox chkSelect = imagePanel.Controls.OfType<CheckBox>().FirstOrDefault();
                        if (chkSelect != null && chkSelect.Checked != shouldBeChecked)
                        {
                            chkSelect.Checked = shouldBeChecked;

                            // Directly update panel and collection state
                            if (shouldBeChecked)
                            {
                                if (!selectedImagePanels.Contains(imagePanel))
                                {
                                    selectedImagePanels.Add(imagePanel);
                                    imagePanel.BackColor = Color.LightBlue;
                                }
                            }
                            else
                            {
                                selectedImagePanels.Remove(imagePanel);
                                imagePanel.BackColor = SystemColors.Control;
                            }
                        }
                    }
                }

                // Update download button visibility
                btnDownloadSelected.Visible = selectedImagePanels.Count > 0;
            }
            finally
            {
                // Always reset lock flag
                _isUpdatingCheckboxes = false;
            }
        }
        private void BtnDownloadSelected_Click(object sender, EventArgs e)
        {
            if (selectedImagePanels.Count > 0)
            {
                DownloadSelectedImages();
            }
            else
            {
                MessageBox.Show("Select images to download.", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private async void DownloadSelectedImages()
        {
            string folderName = $"Image_{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString().Substring(6)}";
            // Сохраняем в папку проекта (AppContext.BaseDirectory)
            string projectPath = AppContext.BaseDirectory;
            string savePath = Path.Combine(projectPath, folderName);

            try
            {
                Directory.CreateDirectory(savePath);

                progressBar.Maximum = selectedImagePanels.Count;
                progressBar.Value = 0;
                progressBar.Visible = true;
                lblStatus.Text = "Downloading selected images...";
                Application.DoEvents();

                foreach (var panel in selectedImagePanels)
                {
                    PictureBox pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox?.Tag is PixabayImage image)
                    {
                        string fileName = Path.Combine(savePath, $"pixabay_{image.Id}.jpg");
                        await DownloadAndSaveImage(image.LargeImageURL, fileName);
                    }
                    progressBar.Value++;
                }

                progressBar.Visible = true;
                lblStatus.Text = $"Selected images successfully saved to folder '{folderName}'";
                MessageBox.Show($"Images saved to '{savePath}'", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear selection after download
                foreach (var panel in selectedImagePanels.ToList())
                {
                    CheckBox chkSelect = panel.Controls.OfType<CheckBox>().FirstOrDefault();
                    if (chkSelect != null)
                    {
                        chkSelect.Checked = false; // This will trigger ChkSelect_CheckedChanged and remove from selectedImagePanels
                    }
                }
                UpdateDownloadButtonVisibility();
                //chkSelectAll.Checked = false;

            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblStatus.Text = $"Download error: {ex.Message}";
                MessageBox.Show($"Error downloading images: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task DownloadAndSaveImage(string imageUrl, string filePath)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(imageUrl))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateSelectAllCheckboxState()
        {
            // Prevent recursion
            if (_isUpdatingCheckboxes)
                return;

            try
            {
                _isUpdatingCheckboxes = true;

                int checkedCount = 0;
                int totalCheckboxes = 0;

                foreach (Control control in flowLayoutPanelImages.Controls)
                {
                    if (control is Panel imagePanel)
                    {
                        CheckBox chkSelect = imagePanel.Controls.OfType<CheckBox>().FirstOrDefault();
                        if (chkSelect != null)
                        {
                            totalCheckboxes++;
                            if (chkSelect.Checked)
                            {
                                checkedCount++;
                            }
                        }
                    }
                }

                if (totalCheckboxes > 0)
                {
                    chkSelectAll.CheckState = (checkedCount == totalCheckboxes) ?
                        CheckState.Checked :
                        (checkedCount > 0) ? CheckState.Indeterminate : CheckState.Unchecked;
                }
                else
                {
                    chkSelectAll.CheckState = CheckState.Unchecked;
                }
            }
            finally
            {
                _isUpdatingCheckboxes = false;
            }
        }

        private void UpdateDownloadButtonVisibility()
        {
            if (btnDownloadSelected != null)
                btnDownloadSelected.Visible = selectedImagePanels.Count > 0;
        }

        private void SafeInvoke(Action action)
        {
            if (InvokeRequired)
                Invoke(action);
            else
                action();
        }

        #region Additional controls

        private ComboBox cmbImageType;
        private ComboBox cmbOrientation;
        private ComboBox cmbCategory;
        private ComboBox cmbColors;
        private ComboBox cmbOrder;
        private ComboBox cmbLanguage;
        private NumericUpDown nudPerPage;
        private NumericUpDown nudMinWidth;
        private NumericUpDown nudMinHeight;
        private CheckBox chkSafeSearch;
        private CheckBox chkEditorsChoice;
        private Label lblStatus;
        private Label lblPageInfo;
        private Button btnNextPage;
        private Button btnPrevPage;

        private void InitializeAdditionalControls()
        {
            int yOffset = 20;

            // --- Filter panel ---
            TableLayoutPanel filterPanel = new TableLayoutPanel
            {
                Location = new Point(24, 30 + yOffset),
                Size = new Size(760, 180),
                ColumnCount = 4,
                RowCount = 6,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            // Language
            filterPanel.Controls.Add(new Label { Text = "Language:", Anchor = AnchorStyles.Left }, 0, 0);
            cmbLanguage = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            cmbLanguage.Items.AddRange(new object[] {
                "en", "cs", "da", "de", "es", "fr", "id", "it", "hu", "nl", "no", "pl",
                "pt", "ro", "sk", "fi", "sv", "tr", "vi", "th", "bg", "ru", "el", "ja", "ko", "zh"
            });
            cmbLanguage.SelectedItem = "en";
            filterPanel.Controls.Add(cmbLanguage, 1, 0);

            // Image type
            filterPanel.Controls.Add(new Label { Text = "Image type:", Anchor = AnchorStyles.Left }, 2, 0);
            cmbImageType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            cmbImageType.Items.AddRange(new object[] { "all", "photo", "illustration", "vector" });
            cmbImageType.SelectedItem = "photo";
            filterPanel.Controls.Add(cmbImageType, 3, 0);

            // Orientation
            filterPanel.Controls.Add(new Label { Text = "Orientation:", Anchor = AnchorStyles.Left }, 0, 1);
            cmbOrientation = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            cmbOrientation.Items.AddRange(new object[] { "all", "horizontal", "vertical" });
            cmbOrientation.SelectedIndex = 2;
            filterPanel.Controls.Add(cmbOrientation, 1, 1);

            // Category
            filterPanel.Controls.Add(new Label { Text = "Category:", Anchor = AnchorStyles.Left }, 2, 1);
            cmbCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            cmbCategory.Items.AddRange(new object[] {
                "", "backgrounds", "fashion", "nature", "science", "education",
                "feelings", "health", "people", "religion", "places", "animals",
                "industry", "computer", "food", "sports", "transportation",
                "travel", "buildings", "business", "music"
            });
            cmbCategory.SelectedIndex = 0;
            filterPanel.Controls.Add(cmbCategory, 3, 1);

            // Color
            filterPanel.Controls.Add(new Label { Text = "Color:", Anchor = AnchorStyles.Left }, 0, 2);
            cmbColors = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            cmbColors.Items.AddRange(new object[] {
                "", "grayscale", "transparent", "red", "orange", "yellow",
                "green", "turquoise", "blue", "lilac", "pink", "white",
                "gray", "black", "brown"
            });
            cmbColors.SelectedIndex = 13;
            filterPanel.Controls.Add(cmbColors, 1, 2);

            // Sort order
            filterPanel.Controls.Add(new Label { Text = "Sort order:", Anchor = AnchorStyles.Left }, 2, 2);
            cmbOrder = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };
            cmbOrder.Items.AddRange(new object[] { "popular", "latest" });
            cmbOrder.SelectedIndex = 1;
            filterPanel.Controls.Add(cmbOrder, 3, 2);

            // Results per page
            filterPanel.Controls.Add(new Label { Text = "Results per page:", Anchor = AnchorStyles.Left }, 0, 3);
            nudPerPage = new NumericUpDown
            {
                Minimum = 3,
                Maximum = 200,
                Value = 100,
                Dock = DockStyle.Fill
            };
            filterPanel.Controls.Add(nudPerPage, 1, 3);

            // Min width
            filterPanel.Controls.Add(new Label { Text = "Min width:", Anchor = AnchorStyles.Left }, 2, 3);
            nudMinWidth = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10000,
                Value = 0,
                Increment = 100,
                Dock = DockStyle.Fill
            };
            filterPanel.Controls.Add(nudMinWidth, 3, 3);

            // Min height
            filterPanel.Controls.Add(new Label { Text = "Min height:", Anchor = AnchorStyles.Left }, 0, 4);
            nudMinHeight = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 10000,
                Value = 0,
                Increment = 100,
                Dock = DockStyle.Fill
            };
            filterPanel.Controls.Add(nudMinHeight, 1, 4);

            // Checkboxes on a separate row (6th row, all 4 columns)
            Panel checkBoxPanel = new Panel { Dock = DockStyle.Fill };
            chkSafeSearch = new CheckBox
            {
                Text = "Safe search",
                Checked = true,
                Location = new Point(0, 0),
                AutoSize = true
            };
            chkEditorsChoice = new CheckBox
            {
                Text = "Editor's choice",
                Checked = false,
                Location = new Point(150, 0),
                AutoSize = true
            };
            checkBoxPanel.Controls.Add(chkSafeSearch);
            checkBoxPanel.Controls.Add(chkEditorsChoice);
            filterPanel.Controls.Add(checkBoxPanel, 0, 5);
            filterPanel.SetColumnSpan(checkBoxPanel, 4);

            // --- Selection panel ---
            Panel selectionPanel = new Panel
            {
                Location = new Point(12, flowLayoutPanelImages.Location.Y + flowLayoutPanelImages.Height + 140),
                Size = new Size(760, 35),
                Height = 40
            };
            chkSelectAll.Text = "Select all";
            chkSelectAll.Location = new Point(10, 10);
            chkSelectAll.AutoSize = true;
            chkSelectAll.Checked = false;
            chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
            selectionPanel.Controls.Add(chkSelectAll);

            btnDownloadSelected = new Button
            {
                Text = "Download selected",
                Size = new Size(150, 25),
                Location = new Point(150, 7)
            };
            btnDownloadSelected.Click += BtnDownloadSelected_Click;
            selectionPanel.Controls.Add(btnDownloadSelected);

            // --- Navigation panel ---
            Panel navPanel = new Panel
            {
                Location = new Point(12, selectionPanel.Location.Y + selectionPanel.Height + 10),
                Size = new Size(860, 30),
                Height = 35
            };
            btnPrevPage = new Button
            {
                Text = "◄ Back",
                Size = new Size(100, 25),
                Location = new Point(0, 0),
                Enabled = false
            };
            btnPrevPage.Click += BtnPrevPage_Click;

            lblPageInfo = new Label
            {
                Text = "Page 1",
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(150, 25),
                Location = new Point(105, 0),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnNextPage = new Button
            {
                Text = "Next ►",
                Size = new Size(100, 25),
                Location = new Point(260, 0),
                Enabled = false
            };
            btnNextPage.Click += BtnNextPage_Click;

            lblStatus = new Label
            {
                Text = "Ready to search",
                AutoSize = true,
                Location = new Point(370, 5)
            };

            navPanel.Controls.Add(btnPrevPage);
            navPanel.Controls.Add(lblPageInfo);
            navPanel.Controls.Add(btnNextPage);
            navPanel.Controls.Add(lblStatus);

            // --- Progress bar ---
            progressBar = new ProgressBar
            {
                Location = new Point(12, navPanel.Location.Y + navPanel.Height + 10),
                Size = new Size(760, 20),
                Visible = true,
                Height = 30
            };

            // --- Rate limit panel ---
            Panel rateLimitPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = SystemColors.ControlLight
            };
            lblRateLimit = new Label
            {
                Text = "Limit: -",
                AutoSize = true,
                Location = new Point(10, 6)
            };
            lblRateRemaining = new Label
            {
                Text = "Remaining: -",
                AutoSize = true,
                Location = new Point(120, 6)
            };
            lblRateReset = new Label
            {
                Text = "Reset: -",
                AutoSize = true,
                Location = new Point(240, 6)
            };
            lblTelegram = new LinkLabel
            {
                Text = "Telegram: @master_cryptocom",
                AutoSize = true,
                Location = new Point(600, 6),
                LinkColor = Color.Blue
            };
            lblTelegram.Links.Add(10, 17, "https://t.me/master_cryptocom");
            lblTelegram.LinkClicked += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Link.LinkData.ToString(),
                    UseShellExecute = true
                });
            };
            rateLimitPanel.Controls.Add(lblRateLimit);
            rateLimitPanel.Controls.Add(lblRateRemaining);
            rateLimitPanel.Controls.Add(lblRateReset);
            rateLimitPanel.Controls.Add(lblTelegram);

            // --- Images panel ---
            flowLayoutPanelImages.Location = new Point(12, filterPanel.Location.Y + filterPanel.Height + 10);
            flowLayoutPanelImages.Size = new Size(760, 520);

            // --- Add to form ---
            this.Controls.Add(filterPanel);
            this.Controls.Add(selectionPanel);
            this.Controls.Add(navPanel);
            this.Controls.Add(progressBar);
            this.Controls.Add(rateLimitPanel);
            this.Controls.Add(flowLayoutPanelImages);

            string keyFile = Path.Combine(AppContext.BaseDirectory, "Pixabay.txt");
            if (File.Exists(keyFile))
            {
                ApiKey = File.ReadAllText(keyFile).Trim();
            }
            else
            {
                string input = ShowApiKeyInputDialog();
                if (string.IsNullOrWhiteSpace(input))
                {
                    ShowCopyableError("API key not entered. The application will be closed.", "Error");
                    Environment.Exit(0);
                }
                ApiKey = input.Trim();
                File.WriteAllText(keyFile, ApiKey);
            }
        }

        private string ShowApiKeyInputDialog()
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 400;
                prompt.Height = 200;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Enter Pixabay API key";
                prompt.StartPosition = FormStartPosition.CenterScreen;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Enter API key:", Width = 340 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
                Button confirmation = new Button() { Text = "OK", Left = 270, Width = 90, Top = 120, DialogResult = DialogResult.OK };

                // New button to get API key
                Button btnGetApiKey = new Button()
                {
                    Text = "Get API Key",
                    Left = 20,
                    Top = 120,
                    Width = 150
                };
                btnGetApiKey.Click += (s, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://pixabay.com/api/docs/",
                        UseShellExecute = true
                    });
                };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(btnGetApiKey);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }
        private void ShowCopyableError(string message, string title = "Error")
        {
            using (Form errorForm = new Form())
            {
                errorForm.Text = title;
                errorForm.Width = 600;
                errorForm.Height = 250;
                errorForm.StartPosition = FormStartPosition.CenterScreen;
                errorForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                errorForm.MinimizeBox = false;
                errorForm.MaximizeBox = false;

                TextBox textBox = new TextBox()
                {
                    Multiline = true,
                    ReadOnly = true,
                    WordWrap = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Fill,
                    Text = message,
                    ShortcutsEnabled = true // allows copying via Ctrl+C
                };

                Button btnClose = new Button()
                {
                    Text = "Close",
                    Dock = DockStyle.Bottom,
                    Height = 35,
                    DialogResult = DialogResult.OK
                };

                errorForm.Controls.Add(textBox);
                errorForm.Controls.Add(btnClose);
                errorForm.AcceptButton = btnClose;

                // Focus on textbox for easy copying
                errorForm.Shown += (s, e) => textBox.Focus();

                errorForm.ShowDialog();
            }
        }
        #endregion
    }

    public class PixabayResponse
    {
        [JsonProperty("totalHits")]
        public int TotalHits { get; set; }

        [JsonProperty("hits")]
        public PixabayImage[] Hits { get; set; }
    }

    public class PixabayImage
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("pageURL")]
        public string PageURL { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("previewURL")]
        public string PreviewURL { get; set; }

        [JsonProperty("webformatURL")]
        public string WebformatURL { get; set; }

        [JsonProperty("largeImageURL")]
        public string LargeImageURL { get; set; }

        [JsonProperty("imageWidth")]
        public int ImageWidth { get; set; }

        [JsonProperty("imageHeight")]
        public int ImageHeight { get; set; }

        [JsonProperty("views")]
        public int Views { get; set; }

        [JsonProperty("downloads")]
        public int Downloads { get; set; }

        [JsonProperty("likes")]
        public int Likes { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("userImageURL")]
        public string UserImageURL { get; set; }
    }
}