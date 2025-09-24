using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace MapaClaseApp.Models
{
    public static class PdfExporter
    {
        /// <summary>
        /// Exporta el mapa de clase a PDF con imágenes
        /// </summary>
        public static bool ExportToPdf(string filePath, List<Student> students, List<Group> groups, Size canvasSize)
        {
            try
            {
                // Configurar codificación (necesario para PdfSharp)
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                
                // Crear documento PDF
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Mapa de Clase";
                document.Info.Author = "Generador de Mapa de Clase";
                document.Info.Subject = $"Distribución de {students.Count} estudiantes en {groups.Count} grupos";
                
                // Crear primera página (imagen del mapa)
                PdfPage page1 = document.AddPage();
                page1.Size = PdfSharp.PageSize.A4;
                page1.Orientation = PdfSharp.PageOrientation.Landscape;
                
                XGraphics gfx1 = XGraphics.FromPdfPage(page1);
                
                // Título y encabezado
                DrawTitle(gfx1, page1, students, groups);
                
                // Crear y dibujar imagen del mapa
                using (var mapImage = CreateMapImage(students, groups, canvasSize))
                {
                    if (mapImage != null)
                    {
                        DrawMapImage(gfx1, page1, mapImage);
                    }
                }
                
                gfx1.Dispose();
                
                // Crear segunda página (detalles de grupos)
                if (groups.Any())
                {
                    PdfPage page2 = document.AddPage();
                    page2.Size = PdfSharp.PageSize.A4;
                    XGraphics gfx2 = XGraphics.FromPdfPage(page2);
                    
                    DrawGroupDetails(gfx2, page2, groups, students);
                    gfx2.Dispose();
                }
                
                // Guardar documento
                document.Save(filePath);
                document.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear PDF: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}");
            }
        }
        
        /// <summary>
        /// Dibuja el título y encabezado del PDF
        /// </summary>
        private static void DrawTitle(XGraphics gfx, PdfPage page, List<Student> students, List<Group> groups)
        {
            // Título principal
            XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
            string title = "MAPA DE CLASE";
            XSize titleSize = gfx.MeasureString(title, titleFont);
            
            double titleX = (page.Width - titleSize.Width) / 2;
            gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(titleX, 40));
            
            // Información
            XFont infoFont = new XFont("Arial", 12, XFontStyle.Regular);
            string info = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm} | Estudiantes: {students.Count} | Grupos: {groups.Count}";
            XSize infoSize = gfx.MeasureString(info, infoFont);
            
            double infoX = (page.Width - infoSize.Width) / 2;
            gfx.DrawString(info, infoFont, XBrushes.DarkGray, new XPoint(infoX, 65));
            
            // Línea separadora
            gfx.DrawLine(XPens.LightGray, 50, 80, page.Width - 50, 80);
        }
        
        /// <summary>
        /// Dibuja la imagen del mapa en el PDF
        /// </summary>
        private static void DrawMapImage(XGraphics gfx, PdfPage page, Bitmap mapImage)
        {
            try
            {
                // Guardar temporalmente la imagen
                string tempPath = Path.GetTempFileName() + ".png";
                mapImage.Save(tempPath, ImageFormat.Png);
                
                // Crear XImage desde archivo temporal
                XImage xImage = XImage.FromFile(tempPath);
                
                // Calcular dimensiones manteniendo aspecto
                double maxWidth = page.Width - 100;  // Márgenes de 50 a cada lado
                double maxHeight = page.Height - 150; // Espacio para título y pie
                
                double scaleX = maxWidth / xImage.PixelWidth;
                double scaleY = maxHeight / xImage.PixelHeight;
                double scale = Math.Min(scaleX, scaleY);
                
                double width = xImage.PixelWidth * scale;
                double height = xImage.PixelHeight * scale;
                
                // Centrar imagen
                double x = (page.Width - width) / 2;
                double y = 100; // Después del título
                
                // Dibujar imagen
                gfx.DrawImage(xImage, x, y, width, height);
                
                // Dibujar borde
                gfx.DrawRectangle(XPens.Gray, x, y, width, height);
                
                // Limpiar archivo temporal
                xImage.Dispose();
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception ex)
            {
                // Si falla, dibujar un rectángulo con texto explicativo
                double x = 50;
                double y = 100;
                double width = page.Width - 100;
                double height = 200;
                
                gfx.DrawRectangle(XPens.Red, x, y, width, height);
                XFont errorFont = new XFont("Arial", 14, XFontStyle.Regular);
                gfx.DrawString($"Error al cargar imagen del mapa:\n{ex.Message}", 
                              errorFont, XBrushes.Red, new XRect(x + 10, y + 10, width - 20, height - 20), XStringFormats.TopLeft);
            }
        }
        
        /// <summary>
        /// Dibuja los detalles de grupos en la segunda página
        /// </summary>
        private static void DrawGroupDetails(XGraphics gfx, PdfPage page, List<Group> groups, List<Student> students)
        {
            XFont titleFont = new XFont("Arial", 20, XFontStyle.Bold);
            XFont headerFont = new XFont("Arial", 12, XFontStyle.Bold);
            XFont normalFont = new XFont("Arial", 10, XFontStyle.Regular);
            
            double y = 50;
            
            // Título de la página
            string title = "DETALLES DE GRUPOS";
            XSize titleSize = gfx.MeasureString(title, titleFont);
            double titleX = (page.Width - titleSize.Width) / 2;
            gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(titleX, y));
            
            y += 40;
            
            // Encabezados de tabla
            double col1X = 50;   // Grupo
            double col2X = 150;  // Cantidad
            double col3X = 220;  // Estudiantes
            
            gfx.DrawString("Grupo", headerFont, XBrushes.Black, new XPoint(col1X, y));
            gfx.DrawString("Cantidad", headerFont, XBrushes.Black, new XPoint(col2X, y));
            gfx.DrawString("Estudiantes", headerFont, XBrushes.Black, new XPoint(col3X, y));
            
            // Línea bajo encabezados
            y += 5;
            gfx.DrawLine(XPens.Black, col1X, y, page.Width - 50, y);
            y += 20;
            
            // Datos de grupos
            foreach (var group in groups.OrderBy(g => g.Id))
            {
                // Verificar si hay espacio en la página
                if (y > page.Height - 100)
                {
                    gfx.DrawString("... continúa", normalFont, XBrushes.Gray, new XPoint(col1X, y));
                    break;
                }
                
                // Nombre del grupo
                gfx.DrawString(group.Label, normalFont, XBrushes.Black, new XPoint(col1X, y));
                
                // Cantidad
                gfx.DrawString(group.Students.Count.ToString(), normalFont, XBrushes.Black, new XPoint(col2X, y));
                
                // Lista de estudiantes
                var studentNames = group.Students.Select(s => s.Name).OrderBy(n => n).ToList();
                string studentsText = string.Join(", ", studentNames);
                
                // Limitar el texto si es muy largo
                if (studentsText.Length > 60)
                {
                    studentsText = studentsText.Substring(0, 57) + "...";
                }
                
                gfx.DrawString(studentsText, normalFont, XBrushes.Black, new XPoint(col3X, y));
                
                y += 20;
            }
            
            // Estudiantes sin grupo
            var studentsWithoutGroup = students.Where(s => s.GroupId == -1).ToList();
            if (studentsWithoutGroup.Any() && y < page.Height - 80)
            {
                y += 20;
                gfx.DrawString("Estudiantes sin grupo:", headerFont, XBrushes.Black, new XPoint(col1X, y));
                y += 20;
                
                string ungroupedText = string.Join(", ", studentsWithoutGroup.Select(s => s.Name).OrderBy(n => n));
                
                // Dividir en múltiples líneas si es necesario
                var words = ungroupedText.Split(' ');
                string currentLine = "";
                
                foreach (string word in words)
                {
                    string testLine = currentLine.Length > 0 ? currentLine + " " + word : word;
                    XSize testSize = gfx.MeasureString(testLine, normalFont);
                    
                    if (testSize.Width > page.Width - col3X - 50)
                    {
                        if (!string.IsNullOrEmpty(currentLine))
                        {
                            gfx.DrawString(currentLine, normalFont, XBrushes.Black, new XPoint(col3X, y));
                            y += 15;
                            currentLine = word;
                        }
                    }
                    else
                    {
                        currentLine = testLine;
                    }
                }
                
                if (!string.IsNullOrEmpty(currentLine))
                {
                    gfx.DrawString(currentLine, normalFont, XBrushes.Black, new XPoint(col3X, y));
                }
            }
        }
        
        /// <summary>
        /// Crea una imagen del mapa completo
        /// </summary>
        private static Bitmap? CreateMapImage(List<Student> students, List<Group> groups, Size canvasSize)
        {
            if (!students.Any()) return null;
            
            try
            {
                // Calcular dimensiones necesarias
                int minX = students.Min(s => s.Position.X) - 20;
                int minY = students.Min(s => s.Position.Y) - 20;
                int maxX = students.Max(s => s.Position.X + s.Size.Width) + 20;
                int maxY = students.Max(s => s.Position.Y + s.Size.Height) + 20;
                
                int width = Math.Max(800, maxX - minX);
                int height = Math.Max(600, maxY - minY);
                
                var bitmap = new Bitmap(width, height);
                
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.White);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    
                    // Ajustar posiciones al offset
                    int offsetX = -minX;
                    int offsetY = -minY;
                    
                    // Dibujar grupos primero
                    foreach (var group in groups.Where(gr => gr.Students.Any()))
                    {
                        DrawGroupInImage(g, group, offsetX, offsetY);
                    }
                    
                    // Dibujar estudiantes
                    foreach (var student in students)
                    {
                        DrawStudentInImage(g, student, offsetX, offsetY);
                    }
                }
                
                return bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear imagen del mapa: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Dibuja un grupo en la imagen
        /// </summary>
        private static void DrawGroupInImage(Graphics g, Group group, int offsetX, int offsetY)
        {
            if (!group.Students.Any()) return;
            
            var adjustedBounds = new System.Drawing.Rectangle(
                group.Bounds.X + offsetX,
                group.Bounds.Y + offsetY,
                group.Bounds.Width,
                group.Bounds.Height
            );
            
            using (Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(60, group.Color)))
            {
                g.FillRectangle(brush, adjustedBounds);
            }
            
            using (Pen pen = new Pen(System.Drawing.Color.FromArgb(150, group.Color), 2))
            {
                g.DrawRectangle(pen, adjustedBounds);
            }
            
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(System.Drawing.Color.Black))
            {
                string label = group.Label;
                SizeF textSize = g.MeasureString(label, font);
                
                var textBg = new RectangleF(
                    adjustedBounds.X + 5,
                    adjustedBounds.Y + 5,
                    textSize.Width + 6,
                    textSize.Height + 2
                );
                
                g.FillRectangle(Brushes.White, textBg);
                g.DrawRectangle(Pens.DarkGray, System.Drawing.Rectangle.Round(textBg));
                
                var textLocation = new PointF(textBg.X + 3, textBg.Y + 1);
                g.DrawString(label, font, textBrush, textLocation);
            }
        }
        
        /// <summary>
        /// Dibuja un estudiante en la imagen
        /// </summary>
        private static void DrawStudentInImage(Graphics g, Student student, int offsetX, int offsetY)
        {
            if (student.Photo == null) return;
            
            var adjustedBounds = new System.Drawing.Rectangle(
                student.Position.X + offsetX,
                student.Position.Y + offsetY,
                student.Size.Width,
                student.Size.Height
            );
            
            // Sombra
            var shadowRect = new System.Drawing.Rectangle(
                adjustedBounds.X + 2, adjustedBounds.Y + 2,
                adjustedBounds.Width, adjustedBounds.Height
            );
            using (Brush shadowBrush = new SolidBrush(System.Drawing.Color.FromArgb(30, System.Drawing.Color.Black)))
            {
                g.FillRectangle(shadowBrush, shadowRect);
            }
            
            // Imagen
            g.DrawImage(student.Photo, adjustedBounds);
            
            // Borde
            g.DrawRectangle(Pens.DarkGray, adjustedBounds);
            
            // Nombre
            using (Font font = new Font("Arial", 8, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(System.Drawing.Color.White))
            using (Brush bgBrush = new SolidBrush(System.Drawing.Color.FromArgb(160, System.Drawing.Color.Black)))
            {
                SizeF textSize = g.MeasureString(student.Name, font);
                var textRect = new RectangleF(
                    adjustedBounds.X,
                    adjustedBounds.Y + adjustedBounds.Height - textSize.Height - 2,
                    adjustedBounds.Width,
                    textSize.Height + 2
                );
                
                g.FillRectangle(bgBrush, textRect);
                
                var textLocation = new PointF(
                    textRect.X + (textRect.Width - textSize.Width) / 2,
                    textRect.Y + 1
                );
                g.DrawString(student.Name, font, textBrush, textLocation);
            }
        }
        
        /// <summary>
        /// Exporta resumen en texto plano
        /// </summary>
        public static bool ExportToText(string filePath, List<Student> students, List<Group> groups)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                
                writer.WriteLine("═══════════════════════════════════");
                writer.WriteLine("         MAPA DE CLASE");
                writer.WriteLine("═══════════════════════════════════");
                writer.WriteLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine($"Total de estudiantes: {students.Count}");
                writer.WriteLine($"Total de grupos: {groups.Count}");
                writer.WriteLine();
                
                if (groups.Any())
                {
                    writer.WriteLine("DISTRIBUCIÓN DE GRUPOS:");
                    writer.WriteLine("─────────────────────────");
                    
                    foreach (var group in groups.OrderBy(g => g.Id))
                    {
                        writer.WriteLine();
                        writer.WriteLine($"┌─ {group.Label.ToUpper()} ─");
                        writer.WriteLine($"│ Estudiantes: {group.Students.Count}");
                        writer.WriteLine("└─ Miembros:");
                        
                        foreach (var student in group.Students.OrderBy(s => s.Name))
                        {
                            writer.WriteLine($"   • {student.Name}");
                        }
                    }
                }
                
                var studentsWithoutGroup = students.Where(s => s.GroupId == -1).ToList();
                if (studentsWithoutGroup.Any())
                {
                    writer.WriteLine();
                    writer.WriteLine("SIN GRUPO ASIGNADO:");
                    writer.WriteLine("─────────────────────");
                    foreach (var student in studentsWithoutGroup.OrderBy(s => s.Name))
                    {
                        writer.WriteLine($"   • {student.Name}");
                    }
                }
                
                writer.WriteLine();
                writer.WriteLine("═══════════════════════════════════");
                writer.WriteLine("    Generado con Mapa de Clase");
                writer.WriteLine("═══════════════════════════════════");
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar a texto: {ex.Message}");
            }
        }
    }
}