namespace Utils
{
    public interface IPositionObserver
    {
        public void Update(Tile tile);
    }

    public interface IPositionSubject
    {
        public void RegisterObserver(IPositionObserver observer);
        public void UnregisterObserver(IPositionObserver observer);
        public void Notify(Tile tile);
    }
}
