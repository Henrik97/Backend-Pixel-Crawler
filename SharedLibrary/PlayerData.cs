namespace SharedLibrary
{
    public class PlayerData
    {
        public int Id { get; set; }
        public Array PlayerCoordinates { get; set; }
    }

    public class PlayerCoordinates
    {
        public string PlayerId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }

}