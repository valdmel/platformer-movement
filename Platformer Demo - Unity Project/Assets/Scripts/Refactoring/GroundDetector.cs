using System;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    private const float BoxAngle = 0F;
    
    public float LastOnGroundTime { get; set; }
    
    [SerializeField] private Transform groundDetectorPoint;
    [SerializeField] private Vector2 groundDetectorSize = new Vector2(0.49f, 0.03f);
    [SerializeField] private LayerMask detectOn;

    private void Update() => LastOnGroundTime -= Time.deltaTime;

    public Collider2D Detect() => Physics2D.OverlapBox(groundDetectorPoint.position, groundDetectorSize, BoxAngle, detectOn);
}