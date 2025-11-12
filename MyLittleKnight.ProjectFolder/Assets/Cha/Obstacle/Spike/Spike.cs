using UnityEngine;
using System.Collections;//코루틴을 사용하기 위해 

public class Spike : MonoBehaviour
{
    private PlayerShield playerShield;//플레이어 방어력스크립트 참조

    //코루틴에서만 사용하는 코루틴의 실행과 중지를 제어하는 '코루틴 전용 변수'
    private Coroutine spikeDamageCoroutine;//타일 위에 있을 때의 데미지 처리 코루틴
    private Coroutine debuffCoroutine;//타일을 나간 후의 디버프 처리 코루틴
                                      

    void OnTriggerEnter2D(Collider2D other)//플레이어가 가시 함정에 들어왔을 때
    {
        if (other.CompareTag ("Player"))//영역에 닿은 오브젝트의 태그 "Player" 를 확인
        {
            playerShield = other.GetComponent<PlayerShield> ();

            if (playerShield == null)//필수 컴포넌트 연결 누락 시 경고 후 즉시 종료 (방어 로직)
            {
                Debug.LogError("Spike: 'Player' 태그를 가진 오브젝트에는 PlayerShield 컴포넌트가 없어! 플레이어 오브젝트를 확인해봐!", other.gameObject);
                return;//함수 종료
            }
           

            if (debuffCoroutine != null)//디버프 중이라면 즉시 중지하고 Spike 피해로 전환
            {
                StopCoroutine(debuffCoroutine);
                debuffCoroutine = null;
            }
            if (spikeDamageCoroutine == null)//가시 위에 있는 동안 데미지 코루틴 시작
            {
                spikeDamageCoroutine = StartCoroutine(ApplySpikeDamage());
                Debug.Log("Spike 함정 밟았어: 즉시 피해 시작!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)//가시 함정을 빠져 나갔을때
    {
        if (other.CompareTag("Player"))//영역에 닿은 오브젝트의 태그 "Player" 를 확인
        {
            if(playerShield == null) return;//null 체크는 계속 유지
            

            if(spikeDamageCoroutine != null)//가시 데미지 즉시 중단
            {
                StopCoroutine(spikeDamageCoroutine);
                spikeDamageCoroutine = null;
                Debug.Log("가시 함정 벗어났어. 즉시 피해 중단");
            }    

            if(debuffCoroutine == null)//벗어나도 받을 도트 데미지(디버프) 시작
                debuffCoroutine = StartCoroutine(ApplyDebuffDamage());
        }                
    }

    IEnumerator ApplySpikeDamage()//밟고 있는 동안 지속적으로 데미지를 주는 코루틴
    {
        while (true)//while (true): 무한 반복(플레이어가 벗어날 때까지 계속 데미지를 주기 위함)
        { 
            if (playerShield == null) break;//두 스크립트 중 하나라도 없으면 함수 종료

            if (ObstacleDifficultyManager.Instance != null)//매 틱마다 ObstacleDifficultyManager에서 현재 데미지 값을 실시간으로 가져옴
            {
                float currentDamage = ObstacleDifficultyManager.Instance.GetCurrentSpikeDamage();
                playerShield.TakeShieldDamage(currentDamage);//쉴드에 먼저 데미지 적용, 방어력이 0이면 체력으로 데미지 이전

                float damageTime = ObstacleDifficultyManager.Instance.GetSpikeDamageInterval();

                yield return new WaitForSeconds(damageTime);//난이도 매니저의 틱 간격 사용
            }                             
            else
            {
                Debug.LogError("ObstacleDifficultyManager가 없어! SpikeDamage 코루틴 중단!", gameObject);
                break;
            }
        }                                                                                         
        spikeDamageCoroutine = null;                        
    }
    IEnumerator ApplyDebuffDamage()//가시함정에서 벗어난 후 남은 시간 동안 데미지를 주는 코루틴
    {
        
        float duration = 0f;//디버프 지속 시간(duration)을 난이도 관리자에게서 직접 가져와서 사용

        if (ObstacleDifficultyManager.Instance != null)
            duration = ObstacleDifficultyManager.Instance.GetCurrentDebuffDuration();
        else
        {
            Debug.LogError("ObstacleDifficultyManager가 연결되지 않았어! 디버프가 적용되지 않아!", gameObject);
            yield break;//ObstacleDifficultyManager 스크립트가 없으면 코루틴 즉시 종료
        }
        Debug.Log($"디버프 시작: {duration}초 동안 지속 (난이도 적용)");

        while (duration > 0)//while (duration > 0): duration이 0보다 클 때까지만 반복(제한된 횟수만큼 데미지를 주기 위함)
        {
            if(playerShield == null) break;

            float damageTick = ObstacleDifficultyManager.Instance.GetSpikeDamageInterval();//틱 간격 가져오기

            float currentDebuffDamage = ObstacleDifficultyManager.Instance.GetCurrentDebuffDamage();

            playerShield.TakeShieldDamage(currentDebuffDamage);//playerShield 스크립트 내부에 방어력이 0이되면 체력으로 보내는 로직이 있어,
                                                               //그래서 PlayerHealth를 직접 선언 안해도 돼

            yield return new WaitForSeconds(damageTick);//여기서 실행을 멈추고 damageInterval 초 후에,
                                                        //재개되어 타이머를 줄이고 다음 틱 데미지를 줌
            duration -= damageTick;
        }
        Debug.Log("디버프 데미지 종료!");
        debuffCoroutine = null;
    }

    public void InitializeSpike(float duration)//외부 SpikeManager스크립트에서 호출될 초기화 함수
    {
        //지정된 시간(duration) 후에 이 오브젝트(가시)를 파괴
        Destroy(gameObject, duration);
    }
}


