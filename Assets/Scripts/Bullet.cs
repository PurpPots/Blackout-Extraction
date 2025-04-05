using UnityEngine;



public class Bullet : MonoBehaviour
{

    public float headshotMultiplier = 1.5f;
    
    private void OnCollisionEnter(Collision objectWeHit)
    {
        if (objectWeHit.gameObject.CompareTag("Target")) //if a collision happened with an object that has a "target" tag
        {
            print("hit " + objectWeHit.gameObject.name + " !");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
        }

        if (objectWeHit.gameObject.CompareTag("Wall"))
        {
            print("hit a wall");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
        }


        if (objectWeHit.gameObject.CompareTag("ZombieHead"))
        {
            if (objectWeHit.transform.root.TryGetComponent<Zombie>(out Zombie zombie))
            {
                zombie.TakeDamage(FindFirstObjectByType<PlayerStats>().GetBulletDamage() * headshotMultiplier, transform.position);
                PlayerStats.Instance.ShowHeadshotEffect();
                print("hit a zombie HEAD");
            }
            Destroy(gameObject);
        }
        
        if (objectWeHit.gameObject.CompareTag("Zombie"))
        {
            if (objectWeHit.gameObject.TryGetComponent<Zombie>(out Zombie zombie))
            {
                zombie.TakeDamage(FindFirstObjectByType<PlayerStats>().GetBulletDamage(), transform.position);
                print("hit a zombie");
            }
            else if (objectWeHit.gameObject.TryGetComponent<TankMiniBoss>(out TankMiniBoss tank))
            {
                tank.TakeDamage(FindFirstObjectByType<PlayerStats>().GetBulletDamage());
                print("hit TANK");
            }
            else if (objectWeHit.gameObject.TryGetComponent<TrollFinalBoss>(out TrollFinalBoss troll))
            {
                troll.TakeDamage(FindFirstObjectByType<PlayerStats>().GetBulletDamage());
                print("hit TROLL");
            }

            Destroy(gameObject);

        }

        if (objectWeHit.gameObject.CompareTag("Ground"))
        {
            print("hit the ground");
            CreateBulletImpactEffect(objectWeHit);
            Destroy(gameObject);
        }
    }


    void CreateBulletImpactEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];

        GameObject hole = Instantiate(
            GlobalReference.Instance.bulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
            );

        hole.transform.SetParent(objectWeHit.gameObject.transform);
    }
}
