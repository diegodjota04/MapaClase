using System.Drawing;
using System.Drawing.Drawing2D;
using MapaClaseApp.Models;
using MapaClaseApp.Extensions;
using System.Diagnostics;

namespace MapaClaseApp.Forms
{
    public partial class ClassMapForm : Form
    {
        #region Variables de Clase
        private List<Student> students = new List<Student>();
        private List<Group> groups = new List<Group>();
        private Student? draggedStudent = null;
        private Point dragOffset;
        private Random random = new Random();
        
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
        private ComboBox? cmbGroupSize;
        #endregion
        
        #region Constructor e Inicialización
        public ClassMapForm()
        {
            InitializeComponent();
            SetupForm();
        }
        
        private void SetupForm()
        {
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
        }
        #endregion
        
        #region Creación de Interfaz de Usuario
        
       

// REEMPLAZAR COMPLETAMENTE el método CreateMenuAndButtons() con esta versión simplificada:

private void CreateMenuAndButtons()
{
    // Panel superior para controles - UNA SOLA LÍNEA
    Panel controlPanel = new Panel
    {
        Dock = DockStyle.Top,
        Height = 70, // Reducido a una línea
        BackColor = Color.FromArgb(245, 245, 245)
    };
    
    int x = 10; // Posición X inicial
    int y = 20; // Centrado verticalmente
    int spacing = 10; // Espacio entre controles
    
    // ===== COMBOBOX 1: HILERAS =====
    Label lblRows = new Label
    {
        Text = "Por hilera:",
        Location = new Point(x, y + 3),
        Size = new Size(60, 20),
        ForeColor = Color.DarkSlateGray,
        Font = new Font("Segoe UI", 8.5F)
    };
    x += 65;
    
    ComboBox cmbRowSize = new ComboBox
    {
        Location = new Point(x, y),
        Size = new Size(45, 25),
        DropDownStyle = ComboBoxStyle.DropDownList,
        BackColor = Color.White,
        Font = new Font("Segoe UI", 9F)
    };
    for (int i = 2; i <= 8; i++)
        cmbRowSize.Items.Add(i);
    cmbRowSize.SelectedIndex = 2; // 4 por defecto
    x += 55;
    
    // ===== COMBOBOX 2: ESTILO =====
    Label lblStyle = new Label
    {
        Text = "Estilo:",
        Location = new Point(x, y + 3),
        Size = new Size(35, 20),
        ForeColor = Color.DarkSlateGray,
        Font = new Font("Segoe UI", 8.5F)
    };
    x += 40;
    
    ComboBox cmbClassroomStyle = new ComboBox
    {
        Location = new Point(x, y),
        Size = new Size(100, 25),
        DropDownStyle = ComboBoxStyle.DropDownList,
        BackColor = Color.White,
        Font = new Font("Segoe UI", 8.5F)
    };
    cmbClassroomStyle.Items.AddRange(new string[] {
        "Hileras",
        "Forma U", 
        "Círculo",
        "Grupos"
    });
    cmbClassroomStyle.SelectedIndex = 0;
    x += 110;
    
    // ===== COMBOBOX 3: TAMAÑO GRUPO =====
    Label lblGroupSize = new Label
    {
        Text = "Tamaño:",
        Location = new Point(x, y + 3),
        Size = new Size(50, 20),
        ForeColor = Color.DarkSlateGray,
        Font = new Font("Segoe UI", 8.5F)
    };
    x += 55;
    
    cmbGroupSize = new ComboBox
    {
        Location = new Point(x, y),
        Size = new Size(45, 25),
        DropDownStyle = ComboBoxStyle.DropDownList,
        BackColor = Color.White,
        Font = new Font("Segoe UI", 9F)
    };
    for (int i = 2; i <= 8; i++)
        cmbGroupSize.Items.Add(i);
    cmbGroupSize.SelectedIndex = 0;
    x += 55;
    
    // ===== SEPARADOR VISUAL =====
    Panel separator = new Panel
    {
        Location = new Point(x, y - 5),
        Size = new Size(2, 35),
        BackColor = Color.LightGray
    };
    x += 12;
    
    // ===== BOTÓN 1: CARGAR IMÁGENES =====
    Button btnLoadImages = new Button
    {
        Text = "📁 Cargar",
        Location = new Point(x, y - 5),
        Size = new Size(80, 35),
        BackColor = Color.FromArgb(70, 130, 180),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };
    btnLoadImages.FlatAppearance.BorderSize = 0;
    btnLoadImages.Click += BtnLoadImages_Click;
    x += 90;
    
    // ===== BOTÓN 2: ORGANIZAR (UNIFICADO) =====
    Button btnOrganize = new Button
    {
        Text = "📐 Organizar",
        Location = new Point(x, y - 5),
        Size = new Size(90, 35),
        BackColor = Color.FromArgb(75, 0, 130),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };
    btnOrganize.FlatAppearance.BorderSize = 0;
    btnOrganize.Click += (s, e) => OrganizeStudents(cmbRowSize, cmbClassroomStyle, cmbGroupSize);
    x += 100;
    
    // ===== BOTÓN 3: GUARDAR PDF =====
    Button btnSavePdf = new Button
    {
        Text = "📄 Guardar",
        Location = new Point(x, y - 5),
        Size = new Size(90, 35),
        BackColor = Color.FromArgb(220, 20, 60),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };
    btnSavePdf.FlatAppearance.BorderSize = 0;
    btnSavePdf.Click += (s, e) => ExportQuickPdf();
    x += 100;
    
    // ===== BOTÓN 4: CARGAR LAYOUT =====
    Button btnLoadLayout = new Button
    {
        Text = "📂 Cargar",
        Location = new Point(x, y - 5),
        Size = new Size(80, 35),
        BackColor = Color.FromArgb(106, 90, 205),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };
    btnLoadLayout.FlatAppearance.BorderSize = 0;
    btnLoadLayout.Click += BtnLoadLayout_Click;
    x += 90;
    
    // ===== BOTÓN 5: LIMPIAR =====
    Button btnClear = new Button
    {
        Text = "🗑️ Limpiar",
        Location = new Point(x, y - 5),
        Size = new Size(80, 35),
        BackColor = Color.FromArgb(255, 140, 0),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };
    btnClear.FlatAppearance.BorderSize = 0;
    btnClear.Click += BtnClearGroups_Click;
    x += 90;
    
    // ===== BOTÓN 6: REINICIAR =====
    Button btnReset = new Button
    {
        Text = "🔄 Reiniciar",
        Location = new Point(x, y - 5),
        Size = new Size(80, 35),
        BackColor = Color.FromArgb(128, 128, 128),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
    };
    btnReset.FlatAppearance.BorderSize = 0;
    btnReset.Click += BtnReset_Click;
    x += 90;
    
    // ===== INFORMACIÓN =====
    Label lblInfo = new Label
    {
        Text = "💡 Configura → Carga fotos → Organiza → Guarda PDF",
        Location = new Point(x + 20, y + 3),
        Size = new Size(300, 20),
        ForeColor = Color.Gray,
        Font = new Font("Segoe UI", 8F, FontStyle.Italic)
    };
    
    // Agregar todos los controles
    controlPanel.Controls.AddRange(new Control[] { 
        lblRows, cmbRowSize, lblStyle, cmbClassroomStyle, lblGroupSize, cmbGroupSize,
        separator, btnLoadImages, btnOrganize, btnSavePdf, btnLoadLayout, btnClear, btnReset, lblInfo
    });
    
    this.Controls.Add(controlPanel);
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
        
        private void LoadStudentImages(string[] filePaths)
        {
            // Limpiar datos existentes
            students.Clear();
            groups.Clear();
            
            int x = 50, y = 90; // espacio panel más grande
            int maxWidth = this.ClientSize.Width - 150;
            int loadedCount = 0;
            
            foreach (string filePath in filePaths)
            {
                try
                {
                    // Obtener nombre sin extensión
                    string studentName = Path.GetFileNameWithoutExtension(filePath);
                    
                    // Cargar y redimensionar imagen
                    using var originalImage = Image.FromFile(filePath);
                    Image resizedImage = ResizeImage(originalImage, 80, 100);
                    
                    // Crear estudiante
                    Student student = new Student(studentName, resizedImage)
                    {
                        Position = new Point(x, y)
                    };
                    
                    students.Add(student);
                    loadedCount++;
                    
                    // Calcular posición siguiente
                    x += 90;
                    if (x > maxWidth)
                    {
                        x = 50;
                        y += 120;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error cargando {Path.GetFileName(filePath)}: {ex.Message}", 
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            
            if (loadedCount > 0)
            {
                MessageBox.Show($"Se cargaron {loadedCount} estudiantes exitosamente.", 
                              "Carga completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            this.Invalidate();
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
                // Actualizar posición
                draggedStudent.Position = new Point(
                    Math.Max(0, Math.Min(e.X - dragOffset.X, this.ClientSize.Width - draggedStudent.Size.Width)),
                    Math.Max(70, Math.Min(e.Y - dragOffset.Y, this.ClientSize.Height - draggedStudent.Size.Height)) // ← CAMBIAR de 60 a 90
                );
                
                // Actualizar grupo si está en uno
                if (draggedStudent.GroupId != -1)
                {
                    var group = groups.FirstOrDefault(g => g.Id == draggedStudent.GroupId);
                    group?.UpdateBounds();
                }
                
                this.Invalidate();
            }
            else
            {
                // Cambiar cursor según contexto
                Student? studentUnderMouse = GetStudentAtPosition(e.Location);
                this.Cursor = studentUnderMouse != null ? Cursors.Hand : Cursors.Default;
            }
        }
        
        private void ClassMapForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (draggedStudent != null)
            {
                CheckGroupTransfer(draggedStudent, e.Location);
                draggedStudent = null;
                this.Cursor = Cursors.Default;
                this.Invalidate();
            }
        }
        
        private Student? GetStudentAtPosition(Point location)
        {
            // Buscar en orden inverso para dar prioridad a estudiantes "encima"
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
        /// Organiza estudiantes en hileras con número específico por fila
        /// </summary>
// ===== AGREGAR ESTOS MÉTODOS EN LA REGIÓN #region Generación de Grupos =====

/// <summary>
/// Método unificado para organizar estudiantes según la configuración seleccionada
/// </summary>
private void OrganizeStudents(ComboBox cmbRowSize, ComboBox cmbClassroomStyle, ComboBox cmbGroupSize)
{
    if (!students.Any())
    {
        MessageBox.Show("Primero carga las imágenes de los estudiantes.", 
                       "Sin estudiantes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }
    
    if (cmbRowSize.SelectedItem == null || cmbClassroomStyle.SelectedItem == null || cmbGroupSize.SelectedItem == null)
    {
        MessageBox.Show("Por favor selecciona todas las opciones antes de organizar.", 
                       "Configuración incompleta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }
    
    int studentsPerRow = (int)cmbRowSize.SelectedItem;
    int groupSize = (int)cmbGroupSize.SelectedItem;
    string style = cmbClassroomStyle.SelectedItem.ToString();
    
    // Limpiar grupos existentes
    groups.Clear();
    students.ForEach(s => s.GroupId = -1);
    
    // Organizar según estilo seleccionado
    switch (style)
    {
        case "Hileras":
            ArrangeStudentsInRows(studentsPerRow);
            ShowOrganizeResult($"Estudiantes organizados en hileras de {studentsPerRow}");
            break;
            
        case "Forma U":
            ArrangeUShapedCorrected();
            ShowOrganizeResult("Estudiantes organizados en forma de U");
            break;
            
        case "Círculo":
            ArrangeCircleCorrected();
            ShowOrganizeResult("Estudiantes organizados en círculo");
            break;
            
        case "Grupos":
            ArrangeInSmallGroupsThenCreateGroups(groupSize);
            ShowOrganizeResult($"Estudiantes organizados en grupos de {groupSize} y grupos creados automáticamente");
            break;
    }
    
    this.Invalidate();
}

/// <summary>
/// Muestra resultado de la organización
/// </summary>
private void ShowOrganizeResult(string message)
{
    MessageBox.Show($"{message}\n\n" +
                   $"Total: {students.Count} estudiantes\n" +
                   $"Grupos activos: {groups.Count}",
                   "Organización Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
}

/// <summary>
/// Disposición en U corregida
/// </summary>
private void ArrangeUShapedCorrected()
{
    var orderedStudents = students.OrderBy(s => s.Name).ToList();
    
    int centerX = this.ClientSize.Width / 2;
    int startY = 120; // Debajo del panel
    int spacing = 100;
    int armLength = Math.Min(6, orderedStudents.Count / 3); // Máximo 6 por brazo
    
    int index = 0;
    
    // BRAZO IZQUIERDO (de arriba hacia abajo)
    int leftArmCount = Math.Min(armLength, orderedStudents.Count - index);
    for (int i = 0; i < leftArmCount; i++)
    {
        int x = centerX - 250; // Posición fija del brazo izquierdo
        int y = startY + (i * spacing);
        orderedStudents[index].Position = new Point(x, y);
        index++;
    }
    
    // BASE DE LA U (de izquierda a derecha)
    int baseY = startY + (leftArmCount * spacing) + 50; // Un poco más abajo
    int studentsInBase = Math.Min(5, orderedStudents.Count - index); // Máximo 5 en la base
    int baseStartX = centerX - 200;
    int baseSpacing = 400 / Math.Max(1, studentsInBase - 1); // Distribuir en 400px
    
    for (int i = 0; i < studentsInBase; i++)
    {
        int x = baseStartX + (i * baseSpacing);
        int y = baseY;
        orderedStudents[index].Position = new Point(x, y);
        index++;
    }
    
    // BRAZO DERECHO (de abajo hacia arriba)
    int rightArmCount = orderedStudents.Count - index;
    for (int i = 0; i < rightArmCount; i++)
    {
        int x = centerX + 250; // Posición fija del brazo derecho
        int y = baseY - 50 - (i * spacing); // Empezar desde abajo hacia arriba
        orderedStudents[index].Position = new Point(x, y);
        index++;
    }
}

/// <summary>
/// Disposición en círculo corregida
/// </summary>
private void ArrangeCircleCorrected()
{
    var orderedStudents = students.OrderBy(s => s.Name).ToList();
    
    int centerX = this.ClientSize.Width / 2;
    int centerY = (this.ClientSize.Height + 120) / 2; // Ajustado por el panel superior
    int radius = Math.Min(centerX - 150, centerY - 100); // Radio más conservador
    
    double angleStep = 2 * Math.PI / orderedStudents.Count;
    
    for (int i = 0; i < orderedStudents.Count; i++)
    {
        double angle = i * angleStep - Math.PI / 2; // Empezar desde arriba (12 en punto)
        
        int x = centerX + (int)(radius * Math.Cos(angle)) - orderedStudents[i].Size.Width / 2;
        int y = centerY + (int)(radius * Math.Sin(angle)) - orderedStudents[i].Size.Height / 2;
        
        // Asegurar que esté dentro de los límites
        x = Math.Max(10, Math.Min(x, this.ClientSize.Width - orderedStudents[i].Size.Width - 10));
        y = Math.Max(120, Math.Min(y, this.ClientSize.Height - orderedStudents[i].Size.Height - 10));
        
        orderedStudents[i].Position = new Point(x, y);
    }
}

/// <summary>
/// Organiza en grupos pequeños Y crea los grupos automáticamente
/// </summary>
private void ArrangeInSmallGroupsThenCreateGroups(int groupSize)
{
    var orderedStudents = students.OrderBy(s => s.Name).ToList();
    
    // ===== CONFIGURACIÓN MEJORADA PARA EVITAR SUPERPOSICIÓN =====
    int groupsPerRow = 2; // Reducido de 3 a 2 para más espacio
    int groupSpacingX = Math.Max(300, this.ClientSize.Width / 3); // Espaciado dinámico
    int groupSpacingY = 280; // Aumentado de 200 a 280
    int studentSpacingX = 100; // Espacio entre estudiantes en el grupo
    int studentSpacingY = 120; // Espacio vertical entre estudiantes
    int startX = 50; // Margen inicial
    int startY = 100; // Debajo del panel
    
    int totalGroups = (int)Math.Ceiling((double)orderedStudents.Count / groupSize);
    int studentIndex = 0;
    
    // Limpiar grupos existentes
    groups.Clear();
    
    // Organizar visualmente y crear grupos
    for (int groupIndex = 0; groupIndex < totalGroups && studentIndex < orderedStudents.Count; groupIndex++)
    {
        // Crear grupo
        Color groupColor = groupColors[groupIndex % groupColors.Length];
        Group group = new Group(groupIndex, groupColor);
        
        // Calcular posición del grupo
        int groupRow = groupIndex / groupsPerRow;
        int groupCol = groupIndex % groupsPerRow;
        
        int groupX = startX + (groupCol * groupSpacingX);
        int groupY = startY + (groupRow * groupSpacingY);
        
        // Verificar que el grupo no se salga de la pantalla
        if (groupX + (2 * studentSpacingX) > this.ClientSize.Width)
        {
            // Si se sale, ajustar a una sola columna
            groupsPerRow = 1;
            groupX = startX;
            groupY = startY + (groupIndex * groupSpacingY);
        }
        
        // Organizar estudiantes dentro del grupo
        int studentsInThisGroup = Math.Min(groupSize, orderedStudents.Count - studentIndex);
        
        // Calcular distribución óptima dentro del grupo
        int studentsPerRowInGroup;
        if (studentsInThisGroup <= 2)
            studentsPerRowInGroup = studentsInThisGroup; // 1 o 2 en una fila
        else if (studentsInThisGroup <= 4)
            studentsPerRowInGroup = 2; // 2x2
        else
            studentsPerRowInGroup = 3; // 3x2 o 3x3
        
        for (int studentInGroup = 0; studentInGroup < studentsInThisGroup; studentInGroup++)
        {
            int studentRow = studentInGroup / studentsPerRowInGroup;
            int studentCol = studentInGroup % studentsPerRowInGroup;
            
            int x = groupX + (studentCol * studentSpacingX);
            int y = groupY + (studentRow * studentSpacingY);
            
            // Asignar posición y grupo
            orderedStudents[studentIndex].Position = new Point(x, y);
            orderedStudents[studentIndex].GroupId = groupIndex;
            group.Students.Add(orderedStudents[studentIndex]);
            
            studentIndex++;
        }
        
        // Actualizar bounds del grupo con margen adicional
        group.UpdateBounds();
        
        // Expandir bounds para evitar superposición
        group.Bounds = new Rectangle(
            group.Bounds.X - 15,
            group.Bounds.Y - 15,
            group.Bounds.Width + 30,
            group.Bounds.Height + 30
        );
        
        groups.Add(group);
    }
}

/// <summary>
/// Versión simplificada del método de hileras (ya existente, pero mejorado)
/// </summary>
private void ArrangeStudentsInRows(int studentsPerRow)
{
    var orderedStudents = students.OrderBy(s => s.Name).ToList();
    
    int startX = 80;
    int startY = 140; // Ajustado para panel más pequeño
    int studentSpacingX = 100;
    int studentSpacingY = 130;
    
    // Ajustar espaciado si no cabe en la pantalla
    int maxWidth = this.ClientSize.Width - 160; // Margen en ambos lados
    int requiredWidth = studentsPerRow * studentSpacingX;
    
    if (requiredWidth > maxWidth)
    {
        studentSpacingX = maxWidth / studentsPerRow;
    }
    
    for (int i = 0; i < orderedStudents.Count; i++)
    {
        int row = i / studentsPerRow;
        int col = i % studentsPerRow;
        
        int x = startX + (col * studentSpacingX);
        int y = startY + (row * studentSpacingY);
        
        orderedStudents[i].Position = new Point(x, y);
    }
}
        



        private void GenerateRandomGroups(int groupSize)
        {
            if (students.Count == 0)
            {
                MessageBox.Show("Primero carga las imágenes de los estudiantes.",
                               "Sin estudiantes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Limpiar grupos existentes
            groups.Clear();
            students.ForEach(s => s.GroupId = -1);

            // Mezclar estudiantes aleatoriamente
            var shuffledStudents = students.OrderBy(s => random.Next()).ToList();

            int groupCount = (int)Math.Ceiling((double)shuffledStudents.Count / groupSize);
            int studentIndex = 0;

            for (int groupId = 0; groupId < groupCount; groupId++)
            {
                // Crear nuevo grupo
                Color groupColor = groupColors[groupId % groupColors.Length];
                Group group = new Group(groupId, groupColor);

                // Asignar estudiantes al grupo
                int studentsInThisGroup = Math.Min(groupSize, shuffledStudents.Count - studentIndex);
                for (int i = 0; i < studentsInThisGroup; i++)
                {
                    var student = shuffledStudents[studentIndex];
                    student.GroupId = groupId;
                    group.Students.Add(student);
                    studentIndex++;
                }

                // Organizar estudiantes en cuadrícula
                OrganizeGroupLayout(group);
                groups.Add(group);
            }

            MessageBox.Show($"Se crearon {groupCount} grupos con {groupSize} estudiantes cada uno.",
                          "Grupos generados", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Invalidate();
        }
        
        private void OrganizeGroupLayout(Group group)
        {
            if (!group.Students.Any()) return;
            
            int studentsPerRow = Math.Min(3, group.Students.Count);
            
            // Calcular posición base del grupo
            int groupsPerRow = 3;
            int groupWidth = 280;
            int groupHeight = 250;
            
            int groupX = 50 + (group.Id % groupsPerRow) * groupWidth;
            int groupY = 100 + (group.Id / groupsPerRow) * groupHeight; // +130 para dejar espacio panel
            
            // Posicionar estudiantes en cuadrícula
            for (int i = 0; i < group.Students.Count; i++)
            {
                int row = i / studentsPerRow;
                int col = i % studentsPerRow;
                
                int x = groupX + col * 90;
                int y = groupY + row * 110;
                
                group.Students[i].Position = new Point(x, y);
            }
            
            group.UpdateBounds();
        }

        #endregion

        #region Transferencia Entre Grupos

        private void CheckGroupTransfer(Student student, Point dropLocation)
        {
            Group? targetGroup = null;

            // Buscar grupo objetivo
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
                // Remover del grupo actual
                if (student.GroupId != -1)
                {
                    var currentGroup = groups.FirstOrDefault(g => g.Id == student.GroupId);
                    if (currentGroup != null)
                    {
                        currentGroup.Students.Remove(student);
                        if (currentGroup.Students.Any())
                        {
                            OrganizeGroupLayout(currentGroup);
                        }
                    }
                }

                // Agregar al nuevo grupo
                student.GroupId = targetGroup.Id;
                targetGroup.Students.Add(student);
                OrganizeGroupLayout(targetGroup);
            }
            
        }
        
        #endregion
        
        #region Renderizado
        
        private void ClassMapForm_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Dibujar grupos primero (fondos)
            foreach (var group in groups.Where(gr => gr.Students.Any()))
            {
                DrawGroup(g, group);
            }
            
            // Dibujar estudiantes encima
            foreach (var student in students)
            {
                DrawStudent(g, student);
            }
            
            // Mostrar información si no hay estudiantes
            if (!students.Any())
            {
                DrawWelcomeMessage(g);
            }
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
            
            // Dibujar fondo del grupo
            using Brush brush = new SolidBrush(group.Color);
            g.FillRoundedRectangle(brush, group.Bounds, 15);
            
            // Dibujar borde del grupo
            using Pen pen = new Pen(Color.FromArgb(200, group.Color), 3);
            g.DrawRoundedRectangle(pen, group.Bounds, 15);
            
            // Dibujar etiqueta del grupo
            using Font font = new Font("Segoe UI", 12, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
            
            string label = group.Label;
            SizeF textSize = g.MeasureString(label, font);
            
            // Fondo para el texto
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
            
            // Dibujar sombra
            Rectangle shadowRect = new Rectangle(
                student.Bounds.X + 3, student.Bounds.Y + 3,
                student.Bounds.Width, student.Bounds.Height
            );
            using Brush shadowBrush = new SolidBrush(Color.FromArgb(50, Color.Black));
            g.FillRectangle(shadowBrush, shadowRect);
            
            // Dibujar imagen del estudiante
            g.DrawImage(student.Photo, student.Bounds);
            
            // Dibujar borde
            Color borderColor = student == draggedStudent ? Color.Red : Color.FromArgb(120, Color.Gray);
            using Pen borderPen = new Pen(borderColor, student == draggedStudent ? 3 : 2);
            g.DrawRectangle(borderPen, student.Bounds);
            
            // Dibujar nombre del estudiante
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
                    // Exportar como PDF con imágenes
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
                    // Exportar como texto plano
                    PdfExporter.ExportToText(saveFileDialog.FileName, students, groups);
                    MessageBox.Show("Resumen exportado exitosamente como archivo de texto.", 
                                  "Texto Exportado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                    
                case ".classmap":
                    // Guardar como archivo de datos (funcionalidad anterior)
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
            // Mostrar información del archivo antes de cargar
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
                
                // Cargar datos
                var data = ClassMapSerializer.LoadFromFile(openFileDialog.FileName);
                if (data != null)
                {
                    // Aplicar datos cargados
                    ClassMapSerializer.ApplyLoadedData(data, students, groups, groupColors);
                    
                    // Actualizar bounds de grupos
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

// Método para exportación rápida a PDF
private void ExportQuickPdf()
{
    if (!students.Any())
    {
        MessageBox.Show("No hay datos para exportar.", "Sin datos", 
                       MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
    }
    
    try
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string fileName = $"Mapa_Clase_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        string filePath = Path.Combine(desktopPath, fileName);
        
        this.Cursor = Cursors.WaitCursor;
        PdfExporter.ExportToPdf(filePath, students, groups, this.ClientSize);
        this.Cursor = Cursors.Default;
        
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
        MessageBox.Show($"Error al exportar PDF:\n\n{ex.Message}", 
                       "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

// Método auxiliar para abrir archivos
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
                    // Liberar recursos de imágenes
                    foreach (var student in students)
                    {
                        student.Photo?.Dispose();
                    }
                    
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
            // Liberar recursos al cerrar
            foreach (var student in students)
            {
                student.Photo?.Dispose();
            }
            base.OnFormClosed(e);
        }
        
        #endregion
    }
}