using System.Drawing;

namespace MapaClaseApp.Models
{
    public class Student
    {
        public string Name { get; set; } = string.Empty;
        public Image? Photo { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; } = new Size(80, 100);
        public Rectangle Bounds => new Rectangle(Position, Size);
        public int GroupId { get; set; } = -1; // -1 = sin grupo
        
        public Student(string name, Image photo)
        {
            Name = name;
            Photo = photo;
            Position = new Point(0, 0);
        }
        
        public Student() { }
    }
}