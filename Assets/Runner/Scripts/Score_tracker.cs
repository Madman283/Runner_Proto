using System.Collections;
using System.Collections.Generic;

public class Score_tracker 
{

    public float score = 0;

    //just in ase if score is reset during testing the varible has a back up 
    public float prev_score = 0;

    //this is to change how much the score is going to be over time being added
    public float point_multiplyer = 1;

    
    public void score_adding(float add_target_speed)
    {
        score = score + (add_target_speed * point_multiplyer);
    }

    public float Score()
    {
        return score;
    }

    public void Reset_score()
    {
        prev_score = score;
        score = 0;
    }
    
}
