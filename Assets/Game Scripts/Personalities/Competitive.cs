using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Competitive : Personality
{  

    protected override void Start()
    {
    base.Start();    // call base class

    minAPM = 350;
    maxAPM = 450;
   
    min_reaction_time = 0.005f; //difference between eye and hand
    max_reaction_time = 0.01f; //difference between eye and hand
    
    min_paddle_safety_distance = 1f;
    max_paddle_safety_distance = 1.3f;
    GenerateValues();
    
    InvokeRepeating("PaddleMovement", 0, (float)60/APM);
    }

  
    public override int MoveHeuristic(){
        //VERY BASIC TEST VERSION, DO BETTER LATER
        float paddleX = paddle.transform.position.x;
        float ballX = ball.transform.position.x;

        
        if(ball.GetComponent<Rigidbody2D>().velocity.y < 0)
        ballX = calcTrajectory();

        float distanceX = paddleX - ballX;

        if(Math.Abs(distanceX) <= paddle_safety_distance)
            return 0;
        
        if(distanceX > paddle_safety_distance)
            return 1;

        if(distanceX < paddle_safety_distance)
            return 2;
        
        return 0;
        
    }

    float calcVerticalTime(){
        
        return (paddle.transform.position.y - ball.transform.position.y)/ball.GetComponent<Rigidbody2D>().velocity.y;
    }
    
    float calcTrajectory(){
    float dropTime = calcVerticalTime();

    float prediction =  ball.transform.position.x + ball.GetComponent<Rigidbody2D>().velocity.x*dropTime;

    if(prediction > 9){
        float aux = prediction - 9;
        prediction = prediction - aux; 
    }
    return prediction;
    }

      public override float[] GetVariables(){
        float[] a = {1, APM, reaction_time, paddle_safety_distance};
        return a;
    }

    
    public override float[] GetGEQ(float paddleDistance, float ballHits, float time, int bricks, int win){
        //I felt content
        float content = 0;
        //I felt skilful
        float skillful = 0;
        //I was fully occupied with the game
        float occupied = 0;
        //I thought it was hard
        float hard = 0;
        //overall enjoyment
        float satisfaction = 0;

        float[] a = {content, skillful, occupied, hard, satisfaction};

        return a;
    }


}
