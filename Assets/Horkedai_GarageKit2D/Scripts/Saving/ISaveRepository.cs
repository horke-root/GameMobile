using System.Threading.Tasks;
using UnityEngine;

public interface ISaveRepository
{
    public void Save<T>(T data);
    public T Load<T>() where T : new();
}
