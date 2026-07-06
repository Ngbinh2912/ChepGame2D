using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public Player playerRef;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            collision.GetComponent<Character>().OnHit(20f);

            if(playerRef != null)
            {
                int currentStep = playerRef.GetComboStep();

                Vector3 textSpawnPos = collision.transform.position + Vector3.up * 2.5f;

                if(currentStep == 0)
                {
                    playerRef.TriggerTiming(collision.transform.position + Vector3.up * 1f);
                }

                if (currentStep == 1)
                {
                    Instantiate(playerRef.GetCombatTextPrefab(), textSpawnPos, Quaternion.identity).OnInit("Combo 2!");
                }
                else if (currentStep == 2)
                {
                    Instantiate(playerRef.GetCombatTextPrefab(), textSpawnPos, Quaternion.identity).OnInit("Combo 3!!!");

                    collision.GetComponent<Enemy>().TakeKnockback(playerRef.transform);
                }
            }
        }

        if(collision.tag == "Player")
        {
            collision.GetComponent<Character>().OnHit(10f);
        }
    }
}
