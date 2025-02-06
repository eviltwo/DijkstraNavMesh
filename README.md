# Dijkstra NavMesh
This is a package for performing pathfinding using Dijkstra's algorithm with Unity's NavMesh.  
It is optimized for scenes where many objects move toward a few targets.

![dijkstra-costs](https://github.com/user-attachments/assets/8c14608d-e86e-4e61-a2bd-e7080a68fc7f)

# UPM
```
https://github.com/eviltwo/DijkstraNavMesh.git?path=DijkstraNavMesh/Assets/DijkstraNavMesh
```

# Features
- Generates a graph from the NavMesh for use in Dijkstra's algorithm processing.  
- Allows multiple goals to be set, generating cost-based graphs for each.  
- Goals can move every frame.  
- Costs for nodes near the goals are constantly updated, while costs for distant nodes are updated with a delay, preventing performance degradation in large scenes.  
- Path calculation is inexpensive, allowing a large number of objects to move toward the goals.

# Support My Work
As a solo developer, your financial support would be greatly appreciated and helps me continue working on this project.
- [Asset Store](https://assetstore.unity.com/publishers/12117)
- [Steam](https://store.steampowered.com/curator/45066588)
- [GitHub Sponsors](https://github.com/sponsors/eviltwo)
