using System.Drawing;
using System.Drawing.Drawing2D;
using MapaClaseApp.Models;
using MapaClaseApp.Extensions;
using System.Diagnostics;
using MapaClaseApp.Utils;

namespace MapaClaseApp.Forms
{
    public partial class ClassMapForm : Form
    {
        #region Constantes de UI
        private const int CONTROL_PANEL_HEIGHT = 70;
        private const int BUTTON_HEIGHT = 35;
        private const int BUTTON_SPACING = 10;
        private const int COMBO_WIDTH_SMALL = 45;
        private const int COMBO_WIDTH_MEDIUM = 100;
        private const int LABEL_HEIGHT = 20;
        #endregion

        #region Constantes de Organización
        private const int DEFAULT_START_X = 80;
        private const int DEFAULT_START_Y = 140;
        private const int STUDENT_SPACING_X = 100;
        private const int STUDENT_SPACING_Y = 130;
        private const int GROUP_MARGIN = 15;
        private const int MAX_STUDENTS_PER_GROUP_ROW = 3;
        private const int MAX_GROUPS_PER_ROW = 2;
        #endregion

        #region Variables de Clase
        private List<Student> students = new List<Student>();
        private List<Group> groups = new List<Group>();
        private Student? draggedStudent = null;
        private Point dragOffset;
        private Random random = new Random();
        private Group? highlightedGroup = null; // Grupo resaltado durante drag
        private bool isDragging = false;        // Estado de arrastre
        
        // Colores para grupos
        private readonly Color[] groupColors = {
            Color.FromArgb(100, 255, 182, 193),  // Rosa claro
            Color.FromArgb(100, 173, 216, 230),  // Azul claro
            Color.FromArgb(100, 144, 238, 144),  // Verde claro
            Color.FromArgb(100, 255, 218, 185),  // Durazno
            Color.FromArgb(100, 221, 160, 221),  // Ciruela
            Color.FromArgb(100, 255, 255, 224),  // Amarillo claro
            Color.FromArgb(100, 255, 160, 122),  // Salmón claro
            Color.FromArgb(100, 176, 196, 222),  // Azul acero claro
        };
        
        // Controles UI
        private ComboBox cmbGroupSize = null!;
        #endregion
        
        #region Constructor e Inicialización
        public ClassMapForm()
        {
            InitializeComponent();
            SetupForm();
        }
        
        private void SetupForm()
        {
            SimpleLogger.LogInfo("Inicializando formulario principal");
            
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                        ControlStyles.UserPaint | 
                        ControlStyles.DoubleBuffer, true);
            
            // Habilitar eventos
            this.MouseDown += ClassMapForm_MouseDown;
            this.MouseMove += ClassMapForm_MouseMove;
            this.MouseUp += ClassMapForm_MouseUp;
            this.Paint += ClassMapForm_Paint;
            
            CreateMenuAndButtons();
            
            SimpleLogger.LogInfo($"Formulario inicializado: {this.Size.Width}x{this.Size.Height}");
        }
        #endregion
        
        #region Creación de Interfaz de Usuario

        /// <summary>
        /// Crea el menú y los botones de control principales
        /// </summary>
        private void CreateMenuAndButtons()
        {
            Panel controlPanel = CreateControlPanel();
            
            // Crear contenedor para organizar controles
            var controls = new List<Control>();
            
            // Agregar secciones de controles
            int currentX = 10;
            currentX = AddConfigurationSection(controls, currentX);
            currentX = AddSeparator(controls, currentX);
            currentX = AddActionButtons(controls, currentX);
            AddInfoLabel(controls, currentX + 20);
            
            // Agregar todos los controles al panel
            controlPanel.Controls.AddRange(controls.ToArray());
            this.Controls.Add(controlPanel);
        }

        /// <summary>
        /// Crea el panel principal de controles
        /// </summary>
        private Panel CreateControlPanel()
        {
            return new Panel
            {
                Dock = DockStyle.Top,
                Height = CONTROL_PANEL_HEIGHT,
                BackColor = Color.FromArgb(245, 245, 245)
            };
        }

        /// <summary>
        /// Agrega la sección de configuración (ComboBoxes)
        /// </summary>
        private int AddConfigurationSection(List<Control> controls, int startX)
        {
            int x = startX;
            int y = 20;
            
            // ComboBox 1: Hileras
            var (lblRows, cmbRows) = CreateLabeledComboBox(
                "Por hilera:", 
                x, y, 60, COMBO_WIDTH_SMALL,
                Enumerable.Range(2, 7).Cast<object>().ToArray(),
                2
            );
            controls.Add(lblRows);
            controls.Add(cmbRows);
            x += 65 + COMBO_WIDTH_SMALL + BUTTON_SPACING;
            
            cmbRows.Name = "cmbRowSize";
            cmbRows.Tag = "RowSize";
            
            // ComboBox 2: Estilo
            var (lblStyle, cmbStyle) = CreateLabeledComboBox(
                "Estilo:",
                x, y, 35, COMBO_WIDTH_MEDIUM,
                new object[] { "Hileras", "Forma U", "Círculo", "Grupos" },
                0
            );
            controls.Add(lblStyle);
            controls.Add(cmbStyle);
            x += 40 + COMBO_WIDTH_MEDIUM + BUTTON_SPACING;
            
            cmbStyle.Name = "cmbClassroomStyle";
            cmbStyle.Tag = "Style";
            
            // ComboBox 3: Tamaño de grupo
            var (lblGroupSize, cmbGroupSizeLocal) = CreateLabeledComboBox(
                "Tamaño:",
                x, y, 50, COMBO_WIDTH_SMALL,
                Enumerable.Range(2, 7).Cast<object>().ToArray(),
                0
            );
            controls.Add(lblGroupSize);
            controls.Add(cmbGroupSizeLocal);
            x += 55 + COMBO_WIDTH_SMALL + BUTTON_SPACING;
            
            this.cmbGroupSize = cmbGroupSizeLocal;
            cmbGroupSizeLocal.Name = "cmbGroupSize";
            cmbGroupSizeLocal.Tag = "GroupSize";
            
            return x;
        }

        /// <summary>
        /// Crea un Label y ComboBox emparejados
        /// </summary>
        private (Label label, ComboBox combo) CreateLabeledComboBox(
            string labelText, int x, int y, int labelWidth, int comboWidth,
            object[] items, int selectedIndex)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(x, y + 3),
                Size = new Size(labelWidth, LABEL_HEIGHT),
                ForeColor = Color.DarkSlateGray,
                Font = new Font("Segoe UI", 8.5F)
            };
            
            var combo = new ComboBox
            {
                Location = new Point(x + labelWidth + 5, y),
                Size = new Size(comboWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White,
                Font = new Font("Segoe UI", comboWidth > 50 ? 8.5F : 9F)
            };
            
            combo.Items.AddRange(items);
            combo.SelectedIndex = selectedIndex;
            
            return (label, combo);
        }

        /// <summary>
        /// Agrega un separador visual
        /// </summary>
        private int AddSeparator(List<Control> controls, int x)
        {
            var separator = new Panel
            {
                Location = new Point(x, 15),
                Size = new Size(2, 35),
                BackColor = Color.LightGray
            };
            controls.Add(separator);
            
            return x + 12;
        }

        /// <summary>
        /// Agrega todos los botones de acción ocn Tooltips
        /// </summary>
        private int AddActionButtons(List<Control> controls, int startX)
        {
            int x = startX;
            int y = 15;
            
            // Crear ToolTip component
            ToolTip toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 200,
                ShowAlways = true,
                ToolTipIcon = ToolTipIcon.Info,
                ToolTipTitle = "Ayuda"
            };
            
            // Botón 1: Cargar Imágenes
            var btnLoadImages = CreateStyledButton("📁 Cargar", x, y, 80, Color.FromArgb(70, 130, 180));
            btnLoadImages.Click += BtnLoadImages_Click;
            toolTip.SetToolTip(btnLoadImages, "Cargar fotos de estudiantes (JPG, PNG, BMP).\nMáximo 100 estudiantes.");
            controls.Add(btnLoadImages);
            x += 80 + BUTTON_SPACING;
            
            // Botón 2: Organizar
            var btnOrganize = CreateStyledButton("📐 Organizar", x, y, 90, Color.FromArgb(75, 0, 130));
            btnOrganize.Click += BtnOrganize_Click;
            toolTip.SetToolTip(btnOrganize, "Organizar estudiantes según el estilo seleccionado.\nConfigura las opciones primero.");
            controls.Add(btnOrganize);
            x += 90 + BUTTON_SPACING;
            
            // Botón 3: Guardar PDF
            var btnSavePdf = CreateStyledButton("📄 Guardar", x, y, 90, Color.FromArgb(220, 20, 60));
            btnSavePdf.Click += BtnSavePdf_Click;
            toolTip.SetToolTip(btnSavePdf, "Exportar mapa a PDF en el escritorio.\nIncluye fotos y lista de grupos.");
            controls.Add(btnSavePdf);
            x += 90 + BUTTON_SPACING;
            
            // Botón 4: Cargar Layout
            var btnLoadLayout = CreateStyledButton("📂 Cargar", x, y, 80, Color.FromArgb(106, 90, 205));
            btnLoadLayout.Click += BtnLoadLayout_Click;
            toolTip.SetToolTip(btnLoadLayout, "Cargar una distribución guardada previamente (.classmap).");
            controls.Add(btnLoadLayout);
            x += 80 + BUTTON_SPACING;
            
            // Botón 5: Limpiar Grupos
            var btnClear = CreateStyledButton("🗑️ Limpiar", x, y, 80, Color.FromArgb(255, 140, 0));
            btnClear.Click += BtnClearGroups_Click;
            toolTip.SetToolTip(btnClear, "Eliminar todos los grupos.\nLos estudiantes mantienen su posición.");
            controls.Add(btnClear);
            x += 80 + BUTTON_SPACING;
            
            // Botón 6: Reiniciar
            var btnReset = CreateStyledButton("🔄 Reiniciar", x, y, 80, Color.FromArgb(128, 128, 128));
            btnReset.Click += BtnReset_Click;
            toolTip.SetToolTip(btnReset, "Reiniciar completamente.\nElimina estudiantes y grupos.");
            controls.Add(btnReset);
            x += 80 + BUTTON_SPACING;
            
            return x;
        }

        /// <summary>
        /// Crea un botón con estilo consistente
        /// </summary>
        private Button CreateStyledButton(string text, int x, int y, int width, Color color)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, BUTTON_HEIGHT),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.2f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.1f);
            
            return button;
        }

        /// <summary>
        /// Agrega la etiqueta de información
        /// </summary>
        private void AddInfoLabel(List<Control> controls, int x)
        {
            var lblInfo = new Label
            {
                Text = "💡 Configura → Carga fotos → Organiza → Guarda PDF",
                Location = new Point(x, 23),
                Size = new Size(300, LABEL_HEIGHT),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F, FontStyle.Italic)
            };
            controls.Add(lblInfo);
        }

        /// <summary>
        /// Manejador del botón Organizar
        /// </summary>
        private void BtnOrganize_Click(object? sender, EventArgs e)
        {
            ComboBox? cmbRowSize = null;
            ComboBox? cmbStyle = null;
            
            foreach (Control control in this.Controls)
            {
                if (control is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is ComboBox combo)
                        {
                            switch (combo.Tag?.ToString())
                            {
                                case "RowSize":
                                    cmbRowSize = combo;
                                    break;
                                case "Style":
                                    cmbStyle = combo;
                                    break;
                            }
                        }
                    }
                }
            }
            
            if (cmbRowSize != null && cmbStyle != null && cmbGroupSize != null)
            {
                OrganizeStudents(cmbRowSize, cmbStyle, cmbGroupSize);
            }
        }

        /// <summary>
        /// Manejador del botón Guardar PDF
        /// </summary>
        private void BtnSavePdf_Click(object? sender, EventArgs e)
        {
            ExportQuickPdf();
        }

        #endregion
        
        #region Carga de Imágenes
        
        private void BtnLoadImages_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos los archivos|*.*",
                Multiselect = true,
                Title = "Seleccionar fotos de estudiantes"
            };
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadStudentImages(openFileDialog.FileNames);
            }
        }
        
// ========================================
// CORRECCIÓN: LoadStudentImages con progressForm correctamente declarado
// Reemplaza todo el método LoadStudentImages con esta versión
// ========================================

private void LoadStudentImages(string[] filePaths)
        {
            const int MAX_STUDENTS = 100;
            
            if (filePaths.Length > MAX_STUDENTS)
            {
                DialogResult result = MessageBox.Show(
                    $"Has seleccionado {filePaths.Length} archivos.\n" +
                    $"El límite es {MAX_STUDENTS} estudiantes.\n\n" +
                    $"¿Cargar solo los primeros {MAX_STUDENTS}?",
                    "Demasiados archivos",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                
                if (result == DialogResult.No) return;
                filePaths = filePaths.Take(MAX_STUDENTS).ToArray();
            }
            
            // DECLARAR progressForm FUERA del bloque para que sea accesible en todo el método
            Form? progressForm = null;
            ProgressBar? progressBar = null;
            Label? lblStatus = null;
            Label? lblCount = null;
            
            // Solo crear el formulario si hay archivos para cargar
            if (filePaths.Length > 0)
            {
                progressForm = new Form
                {
                    Text = "Cargando imágenes...",
                    Size = new Size(400, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                
                lblStatus = new Label
                {
                    Text = "Preparando carga...",
                    Location = new Point(20, 20),
                    Size = new Size(350, 20)
                };
                
                progressBar = new ProgressBar
                {
                    Location = new Point(20, 50),
                    Size = new Size(350, 30),
                    Minimum = 0,
                    Maximum = filePaths.Length,
                    Value = 0
                };
                
                lblCount = new Label
                {
                    Text = "0 / " + filePaths.Length,
                    Location = new Point(20, 85),
                    Size = new Size(350, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                
                progressForm.Controls.AddRange(new Control[] { lblStatus, progressBar, lblCount });
                progressForm.Show();
                Application.DoEvents();
            }
            
            DisposeAllImages();
            students.Clear();
            groups.Clear();
            
            int x = 50, y = 90;
            int maxWidth = this.ClientSize.Width - 150;
            int loadedCount = 0;
            int skippedCount = 0;
            var errors = new List<string>();
            
            for (int i = 0; i < filePaths.Length; i++)
            {
                string filePath = filePaths[i];
                string fileName = Path.GetFileName(filePath);
                
                // Actualizar progreso si el form existe
                if (progressForm != null && lblStatus != null && progressBar != null && lblCount != null)
                {
                    lblStatus.Text = $"Cargando: {fileName}";
                    progressBar.Value = i + 1;
                    lblCount.Text = $"{i + 1} / {filePaths.Length}";
                    Application.DoEvents();
                }
                
                try
                {
                    if (!ValidateImageFile(filePath))
                    {
                        skippedCount++;
                        SimpleLogger.LogWarning($"Archivo inválido saltado: {fileName}");
                        continue;
                    }
                    
                    string studentName = Path.GetFileNameWithoutExtension(filePath);
                    SimpleLogger.LogDebug($"Cargando estudiante: {studentName}");
                    
                    Image resizedImage;
                    using (var fileStream = File.OpenRead(filePath))
                    using (var originalImage = Image.FromStream(fileStream))
                    {
                        resizedImage = ResizeImage(originalImage, 80, 100);
                    }
                    
                    Student student = new Student(studentName, resizedImage)
                    {
                        Position = new Point(x, y)
                    };
                    
                    students.Add(student);
                    loadedCount++;
                    
                    x += 90;
                    if (x > maxWidth)
                    {
                        x = 50;
                        y += 120;
                    }
                }
                catch (OutOfMemoryException oom)
                {
                    SimpleLogger.LogError($"Sin memoria al cargar {fileName}", oom);
                    
                    // Cerrar el formulario de progreso si existe
                    progressForm?.Close();
                    
                    MessageBox.Show(
                        "No hay suficiente memoria para cargar más imágenes.\n" +
                        "Intenta con imágenes más pequeñas o menos archivos.",
                        "Memoria insuficiente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    break;
                }
                catch (Exception ex)
                {
                    SimpleLogger.LogError($"Error cargando {fileName}", ex);
                    errors.Add($"{fileName}: {ex.Message}");
                    skippedCount++;
                }
            }
            
            // Cerrar el formulario de progreso si existe
            progressForm?.Close();
            progressForm?.Dispose();
            
            // Log resumen
            SimpleLogger.LogInfo($"Carga completada: {loadedCount} exitosas, {skippedCount} omitidas");
            
            if (errors.Count > 5)
            {
                SimpleLogger.LogWarning($"Múltiples errores durante la carga: {errors.Count} archivos fallaron");
            }
            
            string message = $"Carga completada:\n" +
                            $"✓ {loadedCount} estudiantes cargados";
            
            if (skippedCount > 0)
            {
                message += $"\n✗ {skippedCount} archivos omitidos";
            }
            
            if (loadedCount > 0)
            {
                MessageBox.Show(message, "Resultado de carga", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.Invalidate();
        }

        // ========================================
        // ALTERNATIVA: Si no quieres usar el formulario de progreso
        // ========================================

        private void LoadStudentImages_Simple(string[] filePaths)
        {
            const int MAX_STUDENTS = 100;
            
            if (filePaths.Length > MAX_STUDENTS)
            {
                DialogResult result = MessageBox.Show(
                    $"Has seleccionado {filePaths.Length} archivos.\n" +
                    $"El límite es {MAX_STUDENTS} estudiantes.\n\n" +
                    $"¿Cargar solo los primeros {MAX_STUDENTS}?",
                    "Demasiados archivos",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                
                if (result == DialogResult.No) return;
                filePaths = filePaths.Take(MAX_STUDENTS).ToArray();
            }
            
            // Mostrar cursor de espera
            this.Cursor = Cursors.WaitCursor;
            
            DisposeAllImages();
            students.Clear();
            groups.Clear();
            
            int x = 50, y = 90;
            int maxWidth = this.ClientSize.Width - 150;
            int loadedCount = 0;
            int skippedCount = 0;
            
            foreach (string filePath in filePaths)
            {
                try
                {
                    if (!ValidateImageFile(filePath))
                    {
                        skippedCount++;
                        continue;
                    }
                    
                    string studentName = Path.GetFileNameWithoutExtension(filePath);
                    
                    Image resizedImage;
                    using (var fileStream = File.OpenRead(filePath))
                    using (var originalImage = Image.FromStream(fileStream))
                    {
                        resizedImage = ResizeImage(originalImage, 80, 100);
                    }
                    
                    Student student = new Student(studentName, resizedImage)
                    {
                        Position = new Point(x, y)
                    };
                    
                    students.Add(student);
                    loadedCount++;
                    
                    x += 90;
                    if (x > maxWidth)
                    {
                        x = 50;
                        y += 120;
                    }
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show(
                        "No hay suficiente memoria para cargar más imágenes.\n" +
                        "Intenta con imágenes más pequeñas o menos archivos.",
                        "Memoria insuficiente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cargando {Path.GetFileName(filePath)}: {ex.Message}", 
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    skippedCount++;
                }
            }
            
            // Restaurar cursor
            this.Cursor = Cursors.Default;
            
            string message = $"Carga completada:\n" +
                            $"✓ {loadedCount} estudiantes cargados";
            
            if (skippedCount > 0)
            {
                message += $"\n✗ {skippedCount} archivos omitidos";
            }
            
            if (loadedCount > 0)
            {
                MessageBox.Show(message, "Resultado de carga", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.Invalidate();
        }       
        private bool ValidateImageFile(string filePath)
        {
            const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
            
            try
            {
                var fileInfo = new FileInfo(filePath);
                
                if (fileInfo.Length > MAX_FILE_SIZE)
                {
                    MessageBox.Show($"El archivo {fileInfo.Name} excede el tamaño máximo de 10MB", 
                                  "Archivo muy grande", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                var extension = fileInfo.Extension.ToLower();
                
                if (!validExtensions.Contains(extension))
                {
                    MessageBox.Show($"Formato no soportado: {extension}", 
                                  "Formato inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validando archivo: {ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        private static Image ResizeImage(Image image, int width, int height)
        {
            Bitmap resized = new Bitmap(width, height);
            using Graphics g = Graphics.FromImage(resized);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, width, height);
            return resized;
        }
        
        #endregion
        
        #region Sistema Drag & Drop
        
        private void ClassMapForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                draggedStudent = GetStudentAtPosition(e.Location);
                if (draggedStudent != null)
                {
                    dragOffset = new Point(
                        e.X - draggedStudent.Position.X,
                        e.Y - draggedStudent.Position.Y
                    );
                    this.Cursor = Cursors.Hand;
                }
            }
        }
        
       private void ClassMapForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (draggedStudent != null && e.Button == MouseButtons.Left)
            {
                isDragging = true;
                
                // Actualizar posición del estudiante
                draggedStudent.Position = new Point(
                    Math.Max(0, Math.Min(e.X - dragOffset.X, this.ClientSize.Width - draggedStudent.Size.Width)),
                    Math.Max(70, Math.Min(e.Y - dragOffset.Y, this.ClientSize.Height - draggedStudent.Size.Height))
                );
                
                // Actualizar grupo si está en uno
                if (draggedStudent.GroupId != -1)
                {
                    var group = groups.FirstOrDefault(g => g.Id == draggedStudent.GroupId);
                    group?.UpdateBounds();
                }
                
                // Detectar grupo bajo el cursor para resaltarlo
                Group? newHighlightedGroup = null;
                foreach (var group in groups)
                {
                    if (group.Bounds.Contains(e.Location) && group.Id != draggedStudent.GroupId)
                    {
                        newHighlightedGroup = group;
                        break;
                    }
                }
                
                // Si cambió el grupo resaltado, redibujar
                if (highlightedGroup != newHighlightedGroup)
                {
                    highlightedGroup = newHighlightedGroup;
                    this.Invalidate();
                }
                
                this.Invalidate();
            }
            else
            {
                // Cambiar cursor según contexto
                Student? studentUnderMouse = GetStudentAtPosition(e.Location);
                this.Cursor = studentUnderMouse != null ? Cursors.Hand : Cursors.Default;
                
                // Si no estamos arrastrando, limpiar el grupo resaltado
                if (highlightedGroup != null)
                {
                    highlightedGroup = null;
                    this.Invalidate();
                }
            }
        }
        
       private void ClassMapForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (draggedStudent != null)
            {
                CheckGroupTransfer(draggedStudent, e.Location);
                draggedStudent = null;
                isDragging = false;
                highlightedGroup = null; // Limpiar grupo resaltado
                this.Cursor = Cursors.Default;
                this.Invalidate();
            }
        }
        
        private Student? GetStudentAtPosition(Point location)
        {
            for (int i = students.Count - 1; i >= 0; i--)
            {
                if (students[i].Bounds.Contains(location))
                    return students[i];
            }
            return null;
        }

        #endregion

        #region Generación de Grupos

        /// <summary>
        /// Método unificado para organizar estudiantes según la configuración seleccionada
        /// </summary>
        private void OrganizeStudents(ComboBox cmbRowSize, ComboBox cmbClassroomStyle, ComboBox cmbGroupSize)
        {
            if (!ValidateOrganizationInputs(cmbRowSize, cmbClassroomStyle, cmbGroupSize))
                return;
            
            // Pattern matching - elegante y seguro
            if (cmbRowSize.SelectedItem is not int studentsPerRow ||
                cmbGroupSize.SelectedItem is not int groupSize ||
                cmbClassroomStyle.SelectedItem is not string style)
            {
                MessageBox.Show("Error en los valores seleccionados.", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            ClearGroupsAndResetStudents();
            
            var organizer = GetOrganizer(style);
            organizer.Organize(students, studentsPerRow, groupSize);
            
            if (style == "Grupos")
            {
                CreateGroupsFromOrganizedStudents(groupSize);
            }
            
            ShowOrganizationResult(style, studentsPerRow, groupSize);
            this.Invalidate();
        }

        private bool ValidateOrganizationInputs(ComboBox cmbRowSize, ComboBox cmbClassroomStyle, ComboBox cmbGroupSize)
        {
            if (!students.Any())
            {
                MessageBox.Show("Primero carga las imágenes de los estudiantes.", 
                               "Sin estudiantes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            
            if (cmbRowSize.SelectedItem == null || cmbClassroomStyle.SelectedItem == null || cmbGroupSize.SelectedItem == null)
            {
                MessageBox.Show("Por favor selecciona todas las opciones antes de organizar.", 
                               "Configuración incompleta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            return true;
        }

        private void ClearGroupsAndResetStudents()
        {
            groups.Clear();
            students.ForEach(s => s.GroupId = -1);
        }

        private void ShowOrganizationResult(string style, int studentsPerRow, int groupSize)
        {
            string message = style switch
            {
                "Hileras" => $"Estudiantes organizados en hileras de {studentsPerRow}",
                "Forma U" => "Estudiantes organizados en forma de U",
                "Círculo" => "Estudiantes organizados en círculo",
                "Grupos" => $"Estudiantes organizados en grupos de {groupSize}",
                _ => "Estudiantes organizados"
            };
            
            MessageBox.Show($"{message}\n\n" +
                           $"Total: {students.Count} estudiantes\n" +
                           $"Grupos activos: {groups.Count}",
                           "Organización Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private IStudentOrganizer GetOrganizer(string style)
        {
            return style switch
            {
                "Hileras" => new RowOrganizer(this),
                "Forma U" => new UShapeOrganizer(this),
                "Círculo" => new CircleOrganizer(this),
                "Grupos" => new GroupOrganizer(this),
                _ => new RowOrganizer(this)
            };
        }

        private void CreateGroupsFromOrganizedStudents(int groupSize)
        {
            groups.Clear();
            
            var studentGroups = students
                .Where(s => s.GroupId != -1)
                .GroupBy(s => s.GroupId)
                .OrderBy(g => g.Key);
            
            foreach (var studentGroup in studentGroups)
            {
                Color groupColor = groupColors[studentGroup.Key % groupColors.Length];
                Group group = new Group(studentGroup.Key, groupColor);
                
                foreach (var student in studentGroup)
                {
                    group.Students.Add(student);
                }
                
                group.UpdateBounds();
                
                group.Bounds = new Rectangle(
                    group.Bounds.X - GROUP_MARGIN,
                    group.Bounds.Y - GROUP_MARGIN,
                    group.Bounds.Width + (GROUP_MARGIN * 2),
                    group.Bounds.Height + (GROUP_MARGIN * 2)
                );
                
                groups.Add(group);
            }
        }

        // Interfaces y clases organizadoras
        private interface IStudentOrganizer
        {
            void Organize(List<Student> students, int parameter1, int parameter2);
        }

        private abstract class BaseOrganizer : IStudentOrganizer
        {
            protected ClassMapForm form;
            
            protected BaseOrganizer(ClassMapForm form)
            {
                this.form = form;
            }
            
            public abstract void Organize(List<Student> students, int parameter1, int parameter2);
            
            protected List<Student> OrderStudentsByName(List<Student> students)
            {
                return students.OrderBy(s => s.Name).ToList();
            }
            
            protected int CalculateAdjustedSpacingX(int studentsPerRow, int defaultSpacing)
            {
                int maxWidth = form.ClientSize.Width - 160;
                int requiredWidth = studentsPerRow * defaultSpacing;
                
                return requiredWidth > maxWidth 
                    ? maxWidth / studentsPerRow 
                    : defaultSpacing;
            }
        }

        private class RowOrganizer : BaseOrganizer
        {
            public RowOrganizer(ClassMapForm form) : base(form) { }
            
            public override void Organize(List<Student> students, int studentsPerRow, int parameter2)
            {
                var orderedStudents = OrderStudentsByName(students);
                int spacingX = CalculateAdjustedSpacingX(studentsPerRow, STUDENT_SPACING_X);
                
                for (int i = 0; i < orderedStudents.Count; i++)
                {
                    int row = i / studentsPerRow;
                    int col = i % studentsPerRow;
                    
                    int x = DEFAULT_START_X + (col * spacingX);
                    int y = DEFAULT_START_Y + (row * STUDENT_SPACING_Y);
                    
                    orderedStudents[i].Position = new Point(x, y);
                }
            }
        }

        private class UShapeOrganizer : BaseOrganizer
        {
            public UShapeOrganizer(ClassMapForm form) : base(form) { }
            
            public override void Organize(List<Student> students, int parameter1, int parameter2)
            {
                var orderedStudents = OrderStudentsByName(students);
                
                int centerX = form.ClientSize.Width / 2;
                int startY = 120;
                int spacing = 100;
                int armLength = Math.Min(6, orderedStudents.Count / 3);
                
                int index = 0;
                
                PlaceStudentsVertically(orderedStudents, ref index, 
                    centerX - 250, startY, spacing, armLength);
                
                int baseY = startY + (armLength * spacing) + 50;
                PlaceStudentsHorizontally(orderedStudents, ref index,
                    centerX - 200, baseY, 400, Math.Min(5, orderedStudents.Count - index));
                
                int remaining = orderedStudents.Count - index;
                for (int i = 0; i < remaining; i++)
                {
                    orderedStudents[index].Position = new Point(
                        centerX + 250,
                        baseY - 50 - (i * spacing)
                    );
                    index++;
                }
            }
            
            private void PlaceStudentsVertically(List<Student> students, ref int index, 
                int x, int startY, int spacing, int count)
            {
                for (int i = 0; i < count && index < students.Count; i++)
                {
                    students[index].Position = new Point(x, startY + (i * spacing));
                    index++;
                }
            }
            
            private void PlaceStudentsHorizontally(List<Student> students, ref int index,
                int startX, int y, int totalWidth, int count)
            {
                if (count <= 0) return;
                
                int spacing = count > 1 ? totalWidth / (count - 1) : 0;
                
                for (int i = 0; i < count && index < students.Count; i++)
                {
                    students[index].Position = new Point(startX + (i * spacing), y);
                    index++;
                }
            }
        }

        private class CircleOrganizer : BaseOrganizer
        {
            public CircleOrganizer(ClassMapForm form) : base(form) { }
            
            public override void Organize(List<Student> students, int parameter1, int parameter2)
            {
                var orderedStudents = OrderStudentsByName(students);
                
                int centerX = form.ClientSize.Width / 2;
                int centerY = (form.ClientSize.Height + 120) / 2;
                int radius = Math.Min(centerX - 150, centerY - 100);
                
                double angleStep = 2 * Math.PI / orderedStudents.Count;
                
                for (int i = 0; i < orderedStudents.Count; i++)
                {
                    double angle = i * angleStep - Math.PI / 2;
                    
                    int x = centerX + (int)(radius * Math.Cos(angle)) - orderedStudents[i].Size.Width / 2;
                    int y = centerY + (int)(radius * Math.Sin(angle)) - orderedStudents[i].Size.Height / 2;
                    
                    x = Math.Max(10, Math.Min(x, form.ClientSize.Width - orderedStudents[i].Size.Width - 10));
                    y = Math.Max(120, Math.Min(y, form.ClientSize.Height - orderedStudents[i].Size.Height - 10));
                    
                    orderedStudents[i].Position = new Point(x, y);
                }
            }
        }

        private class GroupOrganizer : BaseOrganizer
        {
            public GroupOrganizer(ClassMapForm form) : base(form) { }
            
            public override void Organize(List<Student> students, int groupSize, int parameter2)
            {
                var orderedStudents = OrderStudentsByName(students);
                
                int groupSpacingX = Math.Max(300, form.ClientSize.Width / 3);
                int groupSpacingY = 280;
                int totalGroups = (int)Math.Ceiling((double)orderedStudents.Count / groupSize);
                int studentIndex = 0;
                
                for (int groupIndex = 0; groupIndex < totalGroups && studentIndex < orderedStudents.Count; groupIndex++)
                {
                    var groupPosition = CalculateGroupPosition(groupIndex, groupSpacingX, groupSpacingY);
                    int studentsInGroup = Math.Min(groupSize, orderedStudents.Count - studentIndex);
                    
                    PlaceStudentsInGroup(orderedStudents, ref studentIndex, 
                        groupPosition, studentsInGroup, groupIndex);
                }
            }
            
            private Point CalculateGroupPosition(int groupIndex, int spacingX, int spacingY)
            {
                int col = groupIndex % MAX_GROUPS_PER_ROW;
                int row = groupIndex / MAX_GROUPS_PER_ROW;
                
                return new Point(
                    50 + (col * spacingX),
                    100 + (row * spacingY)
                );
            }
            
            private void PlaceStudentsInGroup(List<Student> students, ref int studentIndex,
                Point groupPosition, int count, int groupId)
            {
                int studentsPerRow = count <= 2 ? count : (count <= 4 ? 2 : 3);
                
                for (int i = 0; i < count; i++)
                {
                    int row = i / studentsPerRow;
                    int col = i % studentsPerRow;
                    
                    students[studentIndex].Position = new Point(
                        groupPosition.X + (col * STUDENT_SPACING_X),
                        groupPosition.Y + (row * 120)
                    );
                    students[studentIndex].GroupId = groupId;
                    studentIndex++;
                }
            }
        }

        #endregion

        #region Transferencia Entre Grupos

        private void CheckGroupTransfer(Student student, Point dropLocation)
        {
            Group? targetGroup = default;

            foreach (var group in groups)
            {
                if (group.Bounds.Contains(dropLocation) && group.Id != student.GroupId)
                {
                    targetGroup = group;
                    break;
                }
            }

            if (targetGroup != null)
            {
                if (student.GroupId != -1)
                {
                    Group? currentGroup = groups.FirstOrDefault(g => g.Id == student.GroupId);
                    if (currentGroup != null)
                    {
                        currentGroup.Students.Remove(student);
                        if (currentGroup.Students.Any())
                        {
                            currentGroup.UpdateBounds();
                        }
                    }
                }

                student.GroupId = targetGroup.Id;
                targetGroup.Students.Add(student);
                targetGroup.UpdateBounds();
            }
        }
        
        #endregion
        
        #region Renderizado
        
       private void ClassMapForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Dibujar grupos primero
            foreach (var group in groups.Where(gr => gr.Students.Any()))
            {
                DrawGroup(g, group);
            }
            
            // Dibujar estudiantes
            foreach (var student in students)
            {
                DrawStudent(g, student);
            }
            
            // Mensaje de bienvenida si no hay estudiantes
            if (!students.Any())
            {
                DrawWelcomeMessage(g);
            }
            
            // NUEVO: Dibujar barra de estado
            DrawStatusBar(g);  // ← ESTA ES LA NUEVA LÍNEA
        }
        
        private void DrawWelcomeMessage(Graphics g)
        {
            string message = "👆 Haz clic en 'Cargar Imágenes' para comenzar";
            using Font font = new Font("Segoe UI", 16, FontStyle.Regular);
            using Brush brush = new SolidBrush(Color.Gray);
            
            SizeF textSize = g.MeasureString(message, font);
            PointF location = new PointF(
                (this.ClientSize.Width - textSize.Width) / 2,
                (this.ClientSize.Height - textSize.Height) / 2
            );
            
            g.DrawString(message, font, brush, location);
        }
        
       private void DrawGroup(Graphics g, Group group)
        {
            if (!group.Students.Any()) return;
            
            // Determinar si este grupo está resaltado
            bool isHighlighted = (highlightedGroup != null && highlightedGroup.Id == group.Id);
            
            // Ajustar color si está resaltado
            Color fillColor = isHighlighted 
                ? Color.FromArgb(150, group.Color) // Más opaco cuando está resaltado
                : group.Color;
            
            Color borderColor = isHighlighted
                ? Color.FromArgb(255, group.Color) // Borde más intenso
                : Color.FromArgb(200, group.Color);
            
            int borderWidth = isHighlighted ? 4 : 3;
            
            // Dibujar fondo del grupo
            using Brush brush = new SolidBrush(fillColor);
            g.FillRoundedRectangle(brush, group.Bounds, 15);
            
            // Dibujar borde del grupo (más grueso si está resaltado)
            using Pen pen = new Pen(borderColor, borderWidth);
            if (isHighlighted)
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }
            g.DrawRoundedRectangle(pen, group.Bounds, 15);
            
            // Si está resaltado, agregar efecto de "brillo"
            if (isHighlighted)
            {
                using Brush glowBrush = new SolidBrush(Color.FromArgb(30, Color.Yellow));
                Rectangle glowRect = new Rectangle(
                    group.Bounds.X - 2,
                    group.Bounds.Y - 2,
                    group.Bounds.Width + 4,
                    group.Bounds.Height + 4
                );
                g.FillRoundedRectangle(glowBrush, glowRect, 17);
            }
            
            // Dibujar etiqueta del grupo
            using Font font = new Font("Segoe UI", 12, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
            
            string label = group.Label;
            if (isHighlighted && isDragging)
            {
                label += " (Soltar aquí)";
            }
            
            SizeF textSize = g.MeasureString(label, font);
            
            RectangleF textBg = new RectangleF(
                group.Bounds.X + 10,
                group.Bounds.Y + 5,
                textSize.Width + 10,
                textSize.Height + 4
            );
            
            using Brush bgBrush = new SolidBrush(Color.FromArgb(240, Color.White));
            g.FillRoundedRectangle(bgBrush, Rectangle.Round(textBg), 8);
            
            PointF textLocation = new PointF(textBg.X + 5, textBg.Y + 2);
            g.DrawString(label, font, textBrush, textLocation);
        }
        
        private void DrawStudent(Graphics g, Student student)
        {
            if (student.Photo == null) return;
            
            Rectangle shadowRect = new Rectangle(
                student.Bounds.X + 3, student.Bounds.Y + 3,
                student.Bounds.Width, student.Bounds.Height
            );
            using Brush shadowBrush = new SolidBrush(Color.FromArgb(50, Color.Black));
            g.FillRectangle(shadowBrush, shadowRect);
            
            g.DrawImage(student.Photo, student.Bounds);
            
            Color borderColor = student == draggedStudent ? Color.Red : Color.FromArgb(120, Color.Gray);
            using Pen borderPen = new Pen(borderColor, student == draggedStudent ? 3 : 2);
            g.DrawRectangle(borderPen, student.Bounds);
            
            using Font font = new Font("Segoe UI", 8, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(Color.White);
            using Brush bgBrush = new SolidBrush(Color.FromArgb(180, Color.Black));
            
            SizeF textSize = g.MeasureString(student.Name, font);
            RectangleF textRect = new RectangleF(
                student.Position.X,
                student.Position.Y + student.Size.Height - textSize.Height - 4,
                student.Size.Width,
                textSize.Height + 4
            );
            
            g.FillRectangle(bgBrush, textRect);
            
            PointF textLocation = new PointF(
                textRect.X + (textRect.Width - textSize.Width) / 2,
                textRect.Y + 2
            );
            g.DrawString(student.Name, font, textBrush, textLocation);
        }
        private void DrawStatusBar(Graphics g)
        {
            if (!students.Any()) return;
            
            // Crear área de estado en la parte inferior
            int barHeight = 30;
            Rectangle statusBar = new Rectangle(
                0, 
                this.ClientSize.Height - barHeight,
                this.ClientSize.Width,
                barHeight
            );
            
            // Fondo semi-transparente
            using Brush bgBrush = new SolidBrush(Color.FromArgb(200, 240, 240, 240));
            g.FillRectangle(bgBrush, statusBar);
            
            // Línea superior
            using Pen borderPen = new Pen(Color.Gray, 1);
            g.DrawLine(borderPen, 0, statusBar.Y, this.ClientSize.Width, statusBar.Y);
            
            // Preparar texto
            using Font font = new Font("Segoe UI", 9);
            using Brush textBrush = new SolidBrush(Color.DarkSlateGray);
            
            // Información a mostrar
            string info = $"📚 Estudiantes: {students.Count} | ";
            info += $"👥 Grupos: {groups.Count} | ";
            
            int studentsInGroups = students.Count(s => s.GroupId != -1);
            int studentsFree = students.Count - studentsInGroups;
            
            info += $"✓ En grupos: {studentsInGroups} | ";
            info += $"✗ Libres: {studentsFree}";
            
            // Si está arrastrando, mostrar ayuda
            if (isDragging && draggedStudent != null)
            {
                info += " | 💡 Arrastra sobre un grupo para transferir";
            }
            
            // Dibujar texto
            g.DrawString(info, font, textBrush, new PointF(10, statusBar.Y + 7));
        }
        #endregion
        
        #region Funcionalidades de Guardado y Exportación

        private void BtnSaveLayout_Click(object? sender, EventArgs e)
        {
            if (!students.Any())
            {
                MessageBox.Show("No hay estudiantes para guardar.", 
                               "Sin datos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            using SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo PDF con imágenes (*.pdf)|*.pdf|Archivo de texto plano (*.txt)|*.txt|Archivo de mapa de clase (*.classmap)|*.classmap|Todos los archivos (*.*)|*.*",
                Title = "Exportar/Guardar mapa de clase",
                DefaultExt = "pdf",
                FileName = $"Mapa_Clase_{DateTime.Now:yyyyMMdd_HHmm}"
            };
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string extension = Path.GetExtension(saveFileDialog.FileName).ToLower();
                    
                    switch (extension)
                    {
                        case ".pdf":
                            this.Cursor = Cursors.WaitCursor;
                            PdfExporter.ExportToPdf(saveFileDialog.FileName, students, groups, this.ClientSize);
                            this.Cursor = Cursors.Default;
                            
                            DialogResult pdfResult = MessageBox.Show($"PDF generado exitosamente con imágenes.\n\n" +
                                          $"Estudiantes: {students.Count}\n" +
                                          $"Grupos: {groups.Count}\n" +
                                          $"Archivo: {Path.GetFileName(saveFileDialog.FileName)}\n\n" +
                                          $"¿Deseas abrir el archivo?", 
                                          "PDF Generado", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                            
                            if (pdfResult == DialogResult.Yes)
                            {
                                OpenFile(saveFileDialog.FileName);
                            }
                            break;
                            
                        case ".txt":
                            PdfExporter.ExportToText(saveFileDialog.FileName, students, groups);
                            MessageBox.Show("Resumen exportado exitosamente como archivo de texto.", 
                                          "Texto Exportado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                            
                        case ".classmap":
                            ClassMapSerializer.SaveToFile(saveFileDialog.FileName, students, groups);
                            MessageBox.Show($"Distribución guardada exitosamente.\n\n" +
                                          $"Este archivo puede cargarse posteriormente para restaurar la distribución.", 
                                          "Layout Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                            
                        default:
                            MessageBox.Show("Formato de archivo no soportado.", 
                                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show($"Error al exportar/guardar:\n\n{ex.Message}", 
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnLoadLayout_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivo de mapa de clase (*.classmap)|*.classmap|Todos los archivos (*.*)|*.*",
                Title = "Cargar distribución de clase"
            };
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string summary = ClassMapSerializer.GetFileSummary(openFileDialog.FileName);
                    
                    DialogResult result = MessageBox.Show(
                        $"{summary}\n\n" +
                        $"¿Deseas cargar esta distribución?\n\n" +
                        $"NOTA: Solo se aplicará la distribución a los estudiantes que ya están cargados con el mismo nombre.",
                        "Confirmar carga",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    
                    if (result == DialogResult.Yes)
                    {
                        if (!students.Any())
                        {
                            MessageBox.Show("Primero debes cargar las imágenes de los estudiantes.\n\n" +
                                          "La distribución solo se puede aplicar a estudiantes ya cargados.",
                                          "Sin estudiantes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        var data = ClassMapSerializer.LoadFromFile(openFileDialog.FileName);
                        if (data != null)
                        {
                            ClassMapSerializer.ApplyLoadedData(data, students, groups, groupColors);
                            
                            foreach (var group in groups)
                            {
                                group.UpdateBounds();
                            }
                            
                            this.Invalidate();
                            
                            MessageBox.Show($"Distribución cargada exitosamente.\n\n" +
                                          $"Grupos aplicados: {groups.Count}\n" +
                                          $"Estudiantes reorganizados: {students.Count(s => s.GroupId != -1)}",
                                          "Carga completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar:\n\n{ex.Message}", 
                                  "Error de carga", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnClearGroups_Click(object? sender, EventArgs e)
        {
            if (!groups.Any())
            {
                MessageBox.Show("No hay grupos para limpiar.", 
                               "Sin grupos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            DialogResult result = MessageBox.Show(
                $"¿Deseas eliminar todos los grupos?\n\n" +
                $"Grupos actuales: {groups.Count}\n" +
                $"Estudiantes en grupos: {students.Count(s => s.GroupId != -1)}\n\n" +
                $"Los estudiantes permanecerán en sus posiciones actuales.",
                "Limpiar grupos",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.Yes)
            {
                groups.Clear();
                students.ForEach(s => s.GroupId = -1);
                this.Invalidate();
                
                MessageBox.Show("Grupos eliminados exitosamente.\n\nTodos los estudiantes ahora están libres.", 
                               "Grupos limpiados", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ExportQuickPdf()
        {
            if (!students.Any())
            {
                SimpleLogger.LogWarning("Intento de exportar PDF sin estudiantes");
                MessageBox.Show("No hay datos para exportar.", "Sin datos", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            try
            {
                SimpleLogger.LogInfo($"Iniciando exportación de PDF con {students.Count} estudiantes y {groups.Count} grupos");
                
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"Mapa_Clase_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                string filePath = Path.Combine(desktopPath, fileName);
                
                this.Cursor = Cursors.WaitCursor;
                PdfExporter.ExportToPdf(filePath, students, groups, this.ClientSize);
                this.Cursor = Cursors.Default;
                
                SimpleLogger.LogInfo($"PDF exportado exitosamente: {filePath}");
                
                DialogResult result = MessageBox.Show(
                    $"PDF exportado exitosamente al escritorio:\n\n{fileName}\n\n" +
                    $"El archivo incluye:\n" +
                    $"• Imagen completa del mapa con fotos\n" +
                    $"• Lista detallada de grupos\n" +
                    $"• {students.Count} estudiantes organizados\n\n" +
                    $"¿Deseas abrir el archivo?",
                    "PDF Generado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                
                if (result == DialogResult.Yes)
                {
                    OpenFile(filePath);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                SimpleLogger.LogError("Error al exportar PDF", ex);
                
                MessageBox.Show($"Error al exportar PDF:\n\n{ex.Message}\n\n" +
                            "Revise el log para más detalles.", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenFile(string filePath)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el archivo:\n{ex.Message}", 
                               "Error al abrir", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion
        
        #region Utilidades
        
        private void DisposeAllImages()
        {
            foreach (var student in students)
            {
                student.Photo?.Dispose();
                student.Photo = null;
            }
        }
        
        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (students.Any())
            {
                DialogResult result = MessageBox.Show(
                    "¿Estás seguro de que quieres reiniciar el mapa?\n\nSe perderán todos los grupos y estudiantes cargados.",
                    "Confirmar reinicio",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                
                if (result == DialogResult.Yes)
                {
                    DisposeAllImages();
                    students.Clear();
                    groups.Clear();
                    this.Invalidate();
                }
            }
            else
            {
                MessageBox.Show("No hay nada que reiniciar.", 
                              "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SimpleLogger.LogInfo("Cerrando aplicación");
            SimpleLogger.LogInfo($"Estadísticas finales: {students.Count} estudiantes, {groups.Count} grupos");
            
            DisposeAllImages();
            base.OnFormClosed(e);
            
            SimpleLogger.LogInfo("Aplicación cerrada correctamente");
        }
        
        #endregion
    }
}