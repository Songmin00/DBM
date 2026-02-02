using UnityEngine;

public interface ISurvivorInteractable //발전기, 생존자에 부착
{
    public bool IsSurvivorInteractable { get; set; }
    public void StartSurvivorInteract();
    public void StopSurvivorInteract();
}

public interface IKillerInteractable //발전기, 생존자, 갈고리에 부착
{
    public bool IsKillerInteractable { get; set; }
    public void StartKillerInteract(KillerController killer);
    public void StopKillerInteract();
}
