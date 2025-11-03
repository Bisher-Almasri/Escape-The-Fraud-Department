using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class MazeNavMeshBaker : MonoBehaviour
{
    void Start()
    {
        var surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }
}
