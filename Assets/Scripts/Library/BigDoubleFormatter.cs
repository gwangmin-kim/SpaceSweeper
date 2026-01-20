using System.Collections.Generic;
using System.Text.RegularExpressions; // 문자열 파싱용
using BreakInfinity; // 라이브러리 사용
using UnityEngine;

public static class BigDoubleFormatter
{
    // --- 1. 설정: 사용할 접미사 정의 ---
    // 필요한 만큼 추가하세요. (K, M, B, T, Qa, Qi...)
    // 혹은 aa, ab 방식을 원하면 별도 알고리즘으로 대체 가능합니다.
    private static readonly string[] _suffixes = new string[]
    {
        "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc",
        "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al"
        // ... 계속 추가 가능
    };

    // 파싱 속도를 위한 딕셔너리 (접미사 -> 지수)
    // 예: "K" -> 3, "M" -> 6
    private static Dictionary<string, int> _suffixToExponent;

    static BigDoubleFormatter()
    {
        // 정적 생성자에서 딕셔너리 초기화
        _suffixToExponent = new Dictionary<string, int>();
        for (int i = 0; i < _suffixes.Length; i++)
        {
            if (string.IsNullOrEmpty(_suffixes[i])) continue;

            // "K"는 10^3, "M"은 10^6 ... 즉 인덱스 * 3
            _suffixToExponent.Add(_suffixes[i].ToLower(), i * 3);
        }
    }

    // --- 2. 기능: BigDouble -> String (UI 표시용) ---
    public static string Format(BigDouble value, int decimals = 2)
    {
        // 0 이하 처리
        if (value <= 0) return "0";

        // 1000 미만은 그냥 표시 (예: 999)
        if (value < 1000) return value.ToString("F0");

        // 지수(Exponent)를 기반으로 접미사 인덱스 계산
        // BigDouble.Exponent는 long 타입이므로 int로 캐스팅
        int exponent = (int)value.Exponent;
        int suffixIndex = exponent / 3;

        // 접미사 배열 범위를 넘어가면 과학적 표기법 (e.g. 1.23E+100)
        if (suffixIndex >= _suffixes.Length)
        {
            return value.ToString($"E{decimals}");
        }

        // 숫자 스케일 조정 (예: 1500 -> 1.5)
        // BreakInfinity는 나눗셈 연산 비용이 좀 있으므로,
        // 내부적으로 Mantissa를 조정하는 것보다 그냥 나눠주는 게 구현은 편합니다.
        BigDouble divisor = BigDouble.Pow(10, suffixIndex * 3);
        BigDouble shortValue = value / divisor;

        // 문자열 조립
        return $"{shortValue.ToString($"F{decimals}")}{_suffixes[suffixIndex]}";
    }

    // --- 3. 기능: String -> BigDouble (인스펙터 입력용) ---
    public static BigDouble Parse(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        text = text.Trim(); // 공백 제거

        // 정규식으로 숫자 부분과 문자 부분 분리
        // 예: "1.5 M" -> Group 1: "1.5", Group 2: "M"
        var match = Regex.Match(text, @"^([0-9\.]+)\s*([a-zA-Z]*)$");

        if (!match.Success)
        {
            Debug.LogWarning($"[BigDoubleFormatter] 파싱 실패: {text}. 0을 반환합니다.");
            return 0;
        }

        string numberPart = match.Groups[1].Value;
        string suffixPart = match.Groups[2].Value.ToLower(); // 소문자로 통일

        // 1. 숫자 부분 파싱
        if (!double.TryParse(numberPart, out double number))
        {
            Debug.LogWarning($"[BigDoubleFormatter] 숫자 변환 실패: {numberPart}");
            return 0;
        }

        // 2. 접미사가 없으면 그냥 반환
        if (string.IsNullOrEmpty(suffixPart))
        {
            return new BigDouble(number);
        }

        // 3. 접미사를 지수(Exponent)로 변환
        if (_suffixToExponent.TryGetValue(suffixPart, out int exponent))
        {
            // 공식: 숫자 * 10^지수
            return number * BigDouble.Pow(10, exponent);
        }
        else
        {
            Debug.LogWarning($"[BigDoubleFormatter] 알 수 없는 단위: {suffixPart}. 숫자만 반환합니다.");
            return new BigDouble(number);
        }
    }
}
