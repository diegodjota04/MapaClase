using System.Text.Json;
using System.Drawing;

namespace MapaClaseApp.Models
{
    // Clase para datos serializables (sin referencias de imagen)
    public class ClassMapData
    {
        public List<StudentData> Students { get; set; } = new List<StudentData>();
        public List<GroupData> Groups { get; set; } = new List<GroupData>();
        public DateTime SavedDate { get; set; } = DateTime.Now;
        public string Version { get; set; } = "1.0";
        
        public class StudentData
        {
            public string Name { get; set; } = string.Empty;
            public Point Position { get; set; }
            public int GroupId { get; set; } = -1;
            public string ImagePath { get; set; } = string.Empty; // Ruta original de la imagen
        }
        
        public class GroupData
        {
            public int Id { get; set; }
            public List<string> StudentNames { get; set; } = new List<string>(); // Usar nombres en lugar de índices
            public int ColorArgb { get; set; }
            public Rectangle Bounds { get; set; }
        }
    }
    
    // Clase para manejar guardado y carga
    public static class ClassMapSerializer
    {
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        /// <summary>
        /// Guarda el estado actual del mapa de clase
        /// </summary>
        public static bool SaveToFile(string filePath, List<Student> students, List<Group> groups)
        {
            try
            {
                var data = new ClassMapData
                {
                    SavedDate = DateTime.Now,
                    Version = "1.0"
                };
                
                // Convertir estudiantes a datos serializables
                foreach (var student in students)
                {
                    data.Students.Add(new ClassMapData.StudentData
                    {
                        Name = student.Name,
                        Position = student.Position,
                        GroupId = student.GroupId,
                        ImagePath = "" // No guardamos la ruta por seguridad
                    });
                }
                
                // Convertir grupos a datos serializables
                foreach (var group in groups)
                {
                    var groupData = new ClassMapData.GroupData
                    {
                        Id = group.Id,
                        ColorArgb = group.Color.ToArgb(),
                        Bounds = group.Bounds
                    };
                    
                    // Guardar nombres de estudiantes en lugar de referencias
                    foreach (var student in group.Students)
                    {
                        groupData.StudentNames.Add(student.Name);
                    }
                    
                    data.Groups.Add(groupData);
                }
                
                // Serializar a JSON
                string jsonString = JsonSerializer.Serialize(data, jsonOptions);
                
                // Guardar archivo
                File.WriteAllText(filePath, jsonString);
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar el archivo: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Carga un mapa de clase desde archivo (solo posiciones y grupos, las imágenes deben cargarse después)
        /// </summary>
        public static ClassMapData? LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("El archivo no existe.");
                
                string jsonString = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<ClassMapData>(jsonString, jsonOptions);
                
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar el archivo: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Aplica los datos cargados a las listas existentes de estudiantes
        /// </summary>
        public static bool ApplyLoadedData(ClassMapData data, List<Student> students, List<Group> groups, Color[] groupColors)
        {
            try
            {
                // Limpiar grupos existentes
                groups.Clear();
                
                // Resetear IDs de grupo de todos los estudiantes
                foreach (var student in students)
                {
                    student.GroupId = -1;
                }
                
                // Aplicar posiciones guardadas a estudiantes existentes (por nombre)
                foreach (var savedStudent in data.Students)
                {
                    var existingStudent = students.FirstOrDefault(s => s.Name == savedStudent.Name);
                    if (existingStudent != null)
                    {
                        existingStudent.Position = savedStudent.Position;
                        existingStudent.GroupId = savedStudent.GroupId;
                    }
                }
                
                // Recrear grupos
                foreach (var savedGroup in data.Groups)
                {
                    Color groupColor = groupColors[savedGroup.Id % groupColors.Length];
                    if (savedGroup.ColorArgb != 0)
                    {
                        groupColor = Color.FromArgb(savedGroup.ColorArgb);
                    }
                    
                    var group = new Group(savedGroup.Id, groupColor)
                    {
                        Bounds = savedGroup.Bounds
                    };
                    
                    // Agregar estudiantes al grupo por nombre
                    foreach (var studentName in savedGroup.StudentNames)
                    {
                        var student = students.FirstOrDefault(s => s.Name == studentName);
                        if (student != null)
                        {
                            student.GroupId = savedGroup.Id;
                            group.Students.Add(student);
                        }
                    }
                    
                    // Solo agregar el grupo si tiene estudiantes
                    if (group.Students.Any())
                    {
                        groups.Add(group);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al aplicar los datos cargados: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Crea un resumen del archivo guardado para mostrar información
        /// </summary>
        public static string GetFileSummary(string filePath)
        {
            try
            {
                var data = LoadFromFile(filePath);
                if (data == null) return "Archivo inválido";
                
                return $"Archivo: {Path.GetFileName(filePath)}\n" +
                       $"Guardado: {data.SavedDate:dd/MM/yyyy HH:mm}\n" +
                       $"Estudiantes: {data.Students.Count}\n" +
                       $"Grupos: {data.Groups.Count}\n" +
                       $"Versión: {data.Version}";
            }
            catch
            {
                return "Error al leer el archivo";
            }
        }
        
        /// <summary>
        /// Exporta un resumen en texto plano
        /// </summary>
        public static bool ExportToText(string filePath, List<Student> students, List<Group> groups)
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                
                writer.WriteLine("=== MAPA DE CLASE ===");
                writer.WriteLine($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine($"Total de estudiantes: {students.Count}");
                writer.WriteLine($"Total de grupos: {groups.Count}");
                writer.WriteLine();
                
                if (groups.Any())
                {
                    foreach (var group in groups.OrderBy(g => g.Id))
                    {
                        writer.WriteLine($"--- {group.Label} ---");
                        writer.WriteLine($"Estudiantes: {group.Students.Count}");
                        
                        foreach (var student in group.Students.OrderBy(s => s.Name))
                        {
                            writer.WriteLine($"  • {student.Name}");
                        }
                        
                        writer.WriteLine();
                    }
                }
                
                // Estudiantes sin grupo
                var studentsWithoutGroup = students.Where(s => s.GroupId == -1).ToList();
                if (studentsWithoutGroup.Any())
                {
                    writer.WriteLine("--- Sin Grupo ---");
                    foreach (var student in studentsWithoutGroup.OrderBy(s => s.Name))
                    {
                        writer.WriteLine($"  • {student.Name}");
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar a texto: {ex.Message}");
            }
        }
    }
}