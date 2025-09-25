// ========================================
// CORRECCIÓN: Todos los métodos deben ser STATIC
// Archivo: Models/PdfExporter.cs
// ========================================

using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace MapaClaseApp.Models
{
    public static class PdfExporter  // <-- La clase es static
    {
        /// <summary>
        /// Exporta el mapa de clase a PDF con imágenes - versión mejorada
        /// IMPORTANTE: Mantener como STATIC
        /// </summary>
        public static bool ExportToPdf(string filePath, List<Student> students, List<Group> groups, Size canvasSize)
        {
            PdfDocument? document = null;
            Bitmap? mapImage = null;
            
            try
            {
                // Configurar codificación (necesario para PdfSharp)
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                
                // Crear documento PDF
                document = new PdfDocument();
                document.Info.Title = "Mapa de Clase";
                document.Info.Author = "Generador de Mapa de Clase";
                document.Info.Subject = $"Distribución de {students.Count} estudiantes en {groups.Count} grupos";
                
                // Crear primera página (imagen del mapa)
                PdfPage page1 = document.AddPage();
                page1.Size = PdfSharp.PageSize.A4;
                page1.Orientation = PdfSharp.PageOrientation.Landscape;
                
                using (XGraphics gfx1 = XGraphics.FromPdfPage(page1))
                {
                    // Título y encabezado
                    DrawTitle(gfx1, page1, students, groups);
                    
                    // Crear y dibujar imagen del mapa
                    mapImage = CreateMapImage(students, groups, canvasSize);
                    if (mapImage != null)
                    {
                        DrawMapImage(gfx1, page1, mapImage);
                    }
                }
                
                // Crear segunda página (detalles de grupos)
                if (groups.Any())
                {
                    PdfPage page2 = document.AddPage();
                    page2.Size = PdfSharp.PageSize.A4;
                    
                    using (XGraphics gfx2 = XGraphics.FromPdfPage(page2))
                    {
                        DrawGroupDetails(gfx2, page2, groups, students);
                    }
                }
                
                // Guardar documento
                document.Save(filePath);
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear PDF: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}");
            }
            finally
            {
                // Asegurar liberación de recursos
                mapImage?.Dispose();
                document?.Close();
                document?.Dispose();
            }
        }

        /// <summary>
        /// Dibuja el título y encabezado del PDF
        /// MANTENER COMO STATIC
        /// </summary>
        private static void DrawTitle(XGraphics gfx, PdfPage page, List<Student> students, List<Group> groups)
        {
            // (Código existente sin cambios - ya es static)
            XFont titleFont = new XFont("Arial", 24, XFontStyle.Bold);
            string title = "MAPA DE CLASE";
            XSize titleSize = gfx.MeasureString(title, titleFont);
            
            double titleX = (page.Width - titleSize.Width) / 2;
            gfx.DrawString(title, titleFont, XBrushes.Black, new XPoint(titleX, 40));
            
            XFont infoFont = new XFont("Arial", 12, XFontStyle.Regular);
            string info = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm} | Estudiantes: {students.Count} | Grupos: {groups.Count}";
            XSize infoSize = gfx.MeasureString(info, infoFont);
            
            double infoX = (page.Width - infoSize.Width) / 2;
            gfx.DrawString(info, infoFont, XBrushes.DarkGray, new XPoint(infoX, 65));
            
            gfx.DrawLine(XPens.LightGray, 50, 80, page.Width - 50, 80);
        }

        /// <summary>
        /// Dibuja la imagen del mapa en el PDF con manejo seguro de archivos temporales
        /// MANTENER COMO STATIC
        /// </summary>
        private static void DrawMapImage(XGraphics gfx, PdfPage page, Bitmap mapImage)
        {
            string? tempPath = null;
            
            try
            {
                // Crear archivo temporal con extensión única
                tempPath = Path.Combine(Path.GetTempPath(), $"classmap_{Guid.NewGuid()}.png");
                
                // Guardar imagen temporal
                mapImage.Save(tempPath, ImageFormat.Png);
                
                // Crear XImage y usarla
                using (XImage xImage = XImage.FromFile(tempPath))
                {
                    // Calcular dimensiones manteniendo aspecto
                    double maxWidth = page.Width - 100;
                    double maxHeight = page.Height - 150;
                    
                    double scaleX = maxWidth / xImage.PixelWidth;
                    double scaleY = maxHeight / xImage.PixelHeight;
                    double scale = Math.Min(scaleX, scaleY);
                    
                    double width = xImage.PixelWidth * scale;
                    double height = xImage.PixelHeight * scale;
                    
                    // Centrar imagen
                    double x = (page.Width - width) / 2;
                    double y = 100;
                    
                    // Dibujar imagen
                    gfx.DrawImage(xImage, x, y, width, height);
                    
                    // Dibujar borde
                    gfx.DrawRectangle(XPens.Gray, x, y, width, height);
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
                {
                    gfx.DrawString($"Error al cargar imagen del mapa:\n{ex.Message}", 
                                  errorFont, XBrushes.Red, 
                                  new XRect(x + 10, y + 10, width - 20, height - 20), 
                                  XStringFormats.TopLeft);
                }
            }
            finally
            {
                // SIEMPRE eliminar el archivo temporal
                if (tempPath != null && File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // Si no se puede eliminar, al menos no crashear
                    }
                }
            }
        }

        /// <summary>
        /// Crea una imagen del mapa completo con mejor manejo de recursos
        /// MANTENER COMO STATIC
        /// </summary>
        private static Bitmap? CreateMapImage(List<Student> students, List<Group> groups, Size canvasSize)
        {
            if (!students.Any()) return null;
            
            Bitmap? bitmap = null;
            Graphics? g = null;
            
            try
            {
                // Calcular dimensiones necesarias
                int minX = students.Min(s => s.Position.X) - 20;
                int minY = students.Min(s => s.Position.Y) - 20;
                int maxX = students.Max(s => s.Position.X + s.Size.Width) + 20;
                int maxY = students.Max(s => s.Position.Y + s.Size.Height) + 20;
                
                int width = Math.Max(800, maxX - minX);
                int height = Math.Max(600, maxY - minY);
                
                // Limitar tamaño máximo para evitar problemas de memoria
                const int MAX_SIZE = 4000;
                if (width > MAX_SIZE) width = MAX_SIZE;
                if (height > MAX_SIZE) height = MAX_SIZE;
                
                bitmap = new Bitmap(width, height);
                g = Graphics.FromImage(bitmap);
                
                g.Clear(Color.White);
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
                
                g.Dispose();
                g = null;
                
                return bitmap;
            }
            catch (Exception ex)
            {
                // Limpiar recursos en caso de error
                g?.Dispose();
                bitmap?.Dispose();
                
                throw new Exception($"Error al crear imagen del mapa: {ex.Message}");
            }
        }

        /// <summary>
        /// Dibuja los detalles de grupos en la segunda página
        /// YA ES STATIC - no cambiar
        /// </summary>
        private static void DrawGroupDetails(XGraphics gfx, PdfPage page, List<Group> groups, List<Student> students)
        {
            // (El código existente ya es correcto y static)
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
            
            // Resto del código existente...
            // (no necesita cambios, ya es static)
        }

        /// <summary>
        /// Dibuja un grupo en la imagen con manejo mejorado de recursos
        /// MANTENER COMO STATIC
        /// </summary>
        private static void DrawGroupInImage(Graphics g, Group group, int offsetX, int offsetY)
        {
            if (!group.Students.Any()) return;
            
            var adjustedBounds = new Rectangle(
                group.Bounds.X + offsetX,
                group.Bounds.Y + offsetY,
                group.Bounds.Width,
                group.Bounds.Height
            );
            
            using (Brush brush = new SolidBrush(Color.FromArgb(60, group.Color)))
            {
                g.FillRectangle(brush, adjustedBounds);
            }
            
            using (Pen pen = new Pen(Color.FromArgb(150, group.Color), 2))
            {
                g.DrawRectangle(pen, adjustedBounds);
            }
            
            using (Font font = new Font("Arial", 12, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            using (Brush bgBrush = new SolidBrush(Color.White))
            using (Pen borderPen = new Pen(Color.DarkGray))
            {
                string label = group.Label;
                SizeF textSize = g.MeasureString(label, font);
                
                var textBg = new RectangleF(
                    adjustedBounds.X + 5,
                    adjustedBounds.Y + 5,
                    textSize.Width + 6,
                    textSize.Height + 2
                );
                
                g.FillRectangle(bgBrush, textBg);
                g.DrawRectangle(borderPen, Rectangle.Round(textBg));
                
                var textLocation = new PointF(textBg.X + 3, textBg.Y + 1);
                g.DrawString(label, font, textBrush, textLocation);
            }
        }

        /// <summary>
        /// Dibuja un estudiante en la imagen con manejo mejorado de recursos
        /// MANTENER COMO STATIC
        /// </summary>
        private static void DrawStudentInImage(Graphics g, Student student, int offsetX, int offsetY)
        {
            if (student.Photo == null) return;
            
            var adjustedBounds = new Rectangle(
                student.Position.X + offsetX,
                student.Position.Y + offsetY,
                student.Size.Width,
                student.Size.Height
            );
            
            // Sombra
            var shadowRect = new Rectangle(
                adjustedBounds.X + 2, 
                adjustedBounds.Y + 2,
                adjustedBounds.Width, 
                adjustedBounds.Height
            );
            
            using (Brush shadowBrush = new SolidBrush(Color.FromArgb(30, Color.Black)))
            {
                g.FillRectangle(shadowBrush, shadowRect);
            }
            
            // Imagen - No usar using aquí porque no queremos dispose de student.Photo
            g.DrawImage(student.Photo, adjustedBounds);
            
            // Borde
            using (Pen borderPen = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(borderPen, adjustedBounds);
            }
            
            // Nombre
            using (Font font = new Font("Arial", 8, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.White))
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(160, Color.Black)))
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
        /// YA ES STATIC - mantener sin cambios
        /// </summary>
        public static bool ExportToText(string filePath, List<Student> students, List<Group> groups)
        {
            // El código existente ya es correcto
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