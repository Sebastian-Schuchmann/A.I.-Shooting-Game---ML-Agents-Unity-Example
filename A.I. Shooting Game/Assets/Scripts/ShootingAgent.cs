﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ShootingAgent : Agent
{
    public int score = 0;
    public float speed = 3f;
    
    public Transform shootingPoint;
    public int minStepsBetweenShots = 50;
    public int damage = 100;
    
    private bool ShotAvaliable = true;
    private int StepsUntilShotIsAvaliable = 0;
    
    private Vector3 StartingPosition;
    private Rigidbody Rb;
    
    
    private void Shoot()
    {
        if (!ShotAvaliable)
            return;
        
        var layerMask = 1 << LayerMask.NameToLayer("Enemy");
        var direction = transform.forward;
        
        Debug.Log("Shot");
        Debug.DrawRay(shootingPoint.position, direction * 200f, Color.green, 2f);
        
        if (Physics.Raycast(shootingPoint.position, direction, out var hit, 200f, layerMask))
        {
            hit.transform.GetComponent<Enemy>().GetShot(damage, this);
        }

        ShotAvaliable = false;
        StepsUntilShotIsAvaliable = minStepsBetweenShots;
    }

    private void FixedUpdate()
    {
        if (!ShotAvaliable)
        {
            StepsUntilShotIsAvaliable--;

            if (StepsUntilShotIsAvaliable <= 0)
                ShotAvaliable = true;
        }
        
        AddReward(-1f/MaxStep);
    }
    public override void OnActionReceived(float[] vectorAction)
    {
        if (Mathf.RoundToInt(vectorAction[0]) >= 1)
        {
            Shoot();
        }

        Rb.velocity = new Vector3(vectorAction[1] * speed, 0f, vectorAction[2] * speed);
    }
    
    public override void Initialize()
    {
        StartingPosition = transform.position;
        Rb = GetComponent<Rigidbody>();
        
        //TODO: Delete
        Rb.freezeRotation = true;
    }
    
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetKey(KeyCode.P) ? 1f : 0f;
        actionsOut[2] = Input.GetAxis("Horizontal");
        actionsOut[1] = -Input.GetAxis("Vertical");
        //transform.rotation.SetLookRotation();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode Begin");
        transform.position = StartingPosition;
        Rb.velocity = Vector3.zero;
        ShotAvaliable = true;
    }

    public void RegisterKill()
    {
        score++;
        
        AddReward(1.0f);
        EndEpisode();
    }
}
