using System.Drawing;

namespace MapaClaseApp.Models
{
    public class Group
    {
        public int Id { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public Color Color { get; set; }
        public Rectangle Bounds { get; set; }
        public string Label => $"Grupo {Id + 1}";
        
        public Group(int id, Color color)
        {
            Id = id;
            Color = color;
        }
        
                public void UpdateBounds()
        {
            if (!Students.Any()) 
            {
                Bounds = Rectangle.Empty;
                return;
            }
            
            var minX = Students.Min(s => s.Position.X) - 20; // Aumentar margen
            var minY = Students.Min(s => s.Position.Y) - 35; // Más espacio para etiqueta
            var maxX = Students.Max(s => s.Position.X + s.Size.Width) + 20;
            var maxY = Students.Max(s => s.Position.Y + s.Size.Height) + 20;
            
            Bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }
}