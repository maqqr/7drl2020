using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public static string FixFont(string msg)
        {
            return msg.Replace('0', 'o');
        }

        public static int RollDice(string dmg,bool pos = false) {
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
            return pos ? Mathf.Max(result,0):result;
        }
        public static string GetIndefiniteArticle(string noun_phrase)
        {
            string word = null;
            var m = Regex.Match(noun_phrase, @"\w+");
            if (m.Success)
                word = m.Groups[0].Value;
            else
                return "an";

            var wordi = word.ToLower();
            foreach (string anword in new string[] { "euler", "heir", "honest", "hono" })
                if (wordi.StartsWith(anword))
                    return "an";

            if (wordi.StartsWith("hour") && !wordi.StartsWith("houri"))
                return "an";

            var char_list = new char[] { 'a', 'e', 'd', 'h', 'i', 'l', 'm', 'n', 'o', 'r', 's', 'x' };
            if (wordi.Length == 1)
            {
                if (wordi.IndexOfAny(char_list) == 0)
                    return "an";
                else
                    return "a";
            }

            if (Regex.Match(word, "(?!FJO|[HLMNS]Y.|RY[EO]|SQU|(F[LR]?|[HL]|MN?|N|RH?|S[CHKLMNPTVW]?|X(YL)?)[AEIOU])[FHLMNRSX][A-Z]").Success)
                return "an";

            foreach (string regex in new string[] { "^e[uw]", "^onc?e\b", "^uni([^nmd]|mo)", "^u[bcfhjkqrst][aeiou]" })
            {
                if (Regex.IsMatch(wordi, regex))
                    return "a";
            }

            if (Regex.IsMatch(word, "^U[NK][AIEO]"))
                return "a";
            else if (word == word.ToUpper())
            {
                if (wordi.IndexOfAny(char_list) == 0)
                    return "an";
                else
                    return "a";
            }

            if (wordi.IndexOfAny(new char[] { 'a', 'e', 'i', 'o', 'u' }) == 0)
                return "an";

            if (Regex.IsMatch(wordi, "^y(b[lor]|cl[ea]|fere|gg|p[ios]|rou|tt)"))
                return "an";

            return "a";
        }
    }
}