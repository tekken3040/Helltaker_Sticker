using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucifertaker
{
    class Header
    {
        public const int CHARACTER_NUM = 10;               // 캐릭터 총 숫자
        public const int CHARACTER_ORIGIN_NUM = 9;         // 캐릭터 중복제거 숫자
        public const int MUSIC_NUM = 4;                    // 음악 총 숫자
        public const int ANI_FRAME = 12;                   // 스프라이트 애니메이션 프레임

        // 캐릭터 넘버 이넘
        public enum CharacterSprite
        {
            Azazel = 0,
            Cerberus = 1,
            Cerberus3 = 2,
            Judgement = 3,
            Justice = 4,
            Lucifer = 5,
            LuciferApron = 6,
            Malina = 7,
            Modeus = 8,
            Pandemonica = 9,
            Zdrada = 10,
        };
    }
}
