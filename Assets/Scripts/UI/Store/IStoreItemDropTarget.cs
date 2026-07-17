public interface IStoreItemDropTarget
{
    bool CanAccept(StoreItemData item);
    bool TryAccept(StoreItemData item);
}
