# SubReder
Дипломная работа на тему "Система редактирования субтитров с автоматизированным формированием тайм-кода по видеоданным" Здесь находятся исходные коды программы

Написана на языке C# в MS Visual Studio с использованием графического интерфейса WPF.
Разработано приложение, позволяющее обнаруживать текстовые зоны, распознавать встроенные субтитры на видеопоследовательностях и переводить фразы субтитров, предназначенное для пользователей, работающих с субтитрами.

Используются библиотеки EmguCV для обнаружения текстовых зон и Tesseract для распознавания текста.
Для улучшения точности обнаружения используется нейронная сеть EAST.
Для перевода текста используется API, предоставляемая сервисом MyMemory.

Особенности программного продукта.
Точность обнаружения текстовых зон может достигать 95%. Точность распознавания таких фраз достигает 80%.
Однако чем длинее и качественее используемый видеоматериал, тем дольше работает программа (связано с покадровой обработкой).
Таким образом в среднем на 2-3 минутное видео может уйти от 5 до 10 минут работы алгоритма обнаружения и распознавания встроенных субтитров.
