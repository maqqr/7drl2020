using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Verminator
{
    public static class Utils
    {
        public static Vector2Int ConvertToTileCoord(Vector3 pos)
        {
            int CastCoord(float coord)
            {
                return coord < 0 ? ((int)coord) - 1 : (int)coord;
            }
            return new Vector2Int(CastCoord(pos.x), CastCoord(pos.z));
        }

        public static Vector3 ConvertToUnityCoord(Vector2Int pos)
        {
            return new Vector3(pos.x + 0.5f, 0f, pos.y + 0.5f);
        }

        public static bool IsPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public static bool IsPressed(KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyDown(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDown(KeyCode key)
        {
            return Input.GetKey(key);
        }

        public static bool IsDown(KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKey(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsReleased(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        public static bool IsReleased(KeyCode[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (Input.GetKeyUp(keys[i]))
                {
                    return true;
                }
            }
            return false;
        }
        public static int CalcDamage(string dmg) {
            int result = 0;
            string[] parts = dmg.Split('d');
            int nDice = 0;
            try {
                nDice = int.Parse(parts[0]);
            }
            catch {
                Debug.Log("Unable to parse the amount of dice for "+dmg);
                Debug.Log(parts);
            }
            int modifier =0;
            int dice = 0;
            parts = parts[1].Split('+');
            if (parts.Length<2) {
                parts = parts[0].Split('-');
                try {
                    dice = int.Parse(parts[0]);
                    }
                catch {
                    Debug.Log("Unable to parse the sides of dice for "+dmg);
                    Debug.Log(parts);
                }
                try{
                    modifier = -int.Parse(parts[1]);
                }
                catch {
                    Debug.Log("Unable to parse the - modifier for "+dmg);
                    Debug.Log(parts);
                }
                
            }
            else {
                try {
                    dice = int.Parse(parts[0]);
                    }
                catch {
                    Debug.Log("Unable to parse the sides of dice for "+dmg);
                    Debug.Log(parts);
                }
                try{
                    modifier = int.Parse(parts[1]);
                }
                catch {
                    Debug.Log("Unable to parse the - modifier for "+dmg);
                    Debug.Log(parts);
                }
            }
            for (int i = 0;i<nDice;i++) {
                result += Random.Range(0,dice)+1;

            }
            result += modifier;
            return Mathf.Max(result,0);
        }
    }
}