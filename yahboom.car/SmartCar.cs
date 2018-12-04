using Com.Enterprisecoding.RPI.GPIO;
using Com.Enterprisecoding.RPI.GPIO.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yahboom.car
{
    public class SmartCar
    {

        //定义引脚
        /*灭火电机引脚设置*/
        const int OutfirePin = 8;      //设置灭火电机引脚为wiringPi编码8口

        const int Left_motor_go = 28;       //左电机前进AIN2连接Raspberry的wiringPi编码28口
        const int Left_motor_back = 29;     //左电机后退AIN1连接Raspberry的wiringPi编码29口

        const int Right_motor_go = 24;      //右电机前进BIN2连接Raspberry的wiringPi编码24口
        const int Right_motor_back = 25;    //右电机后退BIN1连接Raspberry的wiringPi编码25口

        const int Left_motor_pwm = 27;      //左电机控速PWMA连接Raspberry的wiringPi编码27口
        const int Right_motor_pwm = 23;     //右电机控速PWMB连接Raspberry的wiringPi编码23口

        const int buzzer = 10; //蜂鸣器引脚设置

        /*RGBLED引脚设置*/
        const int LED_R = 3;           //LED_R接在Raspberry上的wiringPi编码3口
        const int LED_G = 2;           //LED_G接在Raspberry上的wiringPi编码2口
        const int LED_B = 5;           //LED_B接在Raspberry上的wiringPi编码5口

        /*避障红外传感器引脚及变量设置*/
        const int AvoidSensorLeft = 26; //定义左边避障的红外传感器引脚为wiringPi编码26口
        const int AvoidSensorRight = 0;  //定义右边避障的红外传感器引脚为wiringPi编码0口

        /*循迹红外传感器引脚及变量设置*/
        const int TrackSensorLeftPin1 = 9;    //定义左边第一个循迹红外传感器引脚为wiringPi编码9口
        const int TrackSensorLeftPin2 = 21;  //定义左边第二个循迹红外传感器引脚为wiringPi编码21口
        const int TrackSensorRightPin1 = 7;   //定义右边第一个循迹红外传感器引脚为wiringPi编码7口
        const int TrackSensorRightPin2 = 1;   //定义右边第二个循迹红外传感器引脚为wiringPi编码1口

        /*定义光敏电阻引脚及变量设置*/
        const int LdrSensorLeft = 11;   //定义左边光敏电阻引脚为wiringPi编码11口
        const int LdrSensorRight = 22;   //定义右边光敏电阻引脚为wiringPi编码22口

        /*超声波引脚及变量设置*/
        const int EchoPin = 30;         //定义回声脚为连接Raspberry的wiringPi编码30口
        const int TrigPin = 31;         //定义触发脚为连接Raspberry的wiringPi编码31口


        /*设置舵机驱动引脚*/
        const int FrontServoPin = 4;
        const int ServoUpDownPin = 13;
        const int ServoLeftRightPin = 14;

        const uint HIGH = 1;
        const uint LOW = 0;

        int CurrnetSpeed = 100;
        public int Mode { get; set; } = 1;

        public SmartCar() {
            WiringPi.Core.Setup();
            digitalWrite(OutfirePin, HIGH);
            //初始化电机驱动IO为输出方式
            pinMode(Left_motor_go, PinMode.Output);
            pinMode(Left_motor_back, PinMode.Output);
            pinMode(Right_motor_go, PinMode.Output);
            pinMode(Right_motor_back, PinMode.Output);


            pinMode(LED_R, PinMode.Output);
            pinMode(LED_G, PinMode.Output);
            pinMode(LED_B, PinMode.Output);

            pinMode(buzzer, PinMode.Output);

            pinMode(TrigPin, PinMode.Output);  //定义超声波输出脚
            pinMode(OutfirePin, PinMode.Output);//定义灭火IO口为输出模式并初始化

            //初始化舵机引脚为输出模式
            pinMode(FrontServoPin, PinMode.Output);
            pinMode(ServoUpDownPin, PinMode.Output);
            pinMode(ServoLeftRightPin, PinMode.Output);



            //定义光敏电阻引脚为输入模式
            pinMode(LdrSensorLeft, PinMode.Input);
            pinMode(LdrSensorRight, PinMode.Input);

            //创建两个软件控制的PWM脚
            softPwmCreate(Left_motor_pwm, 0, 255);
            softPwmCreate(Right_motor_pwm, 0, 255);

            softPwmCreate(LED_R, 0, 255);
            softPwmCreate(LED_G, 0, 255);
            softPwmCreate(LED_B, 0, 255);

            //定义左右传感器为输入接口
            pinMode(AvoidSensorLeft, PinMode.Input);
            pinMode(AvoidSensorRight, PinMode.Input);

            //定义寻迹红外传感器为输入模式
            pinMode(TrackSensorLeftPin1, PinMode.Input);
            pinMode(TrackSensorLeftPin2, PinMode.Input);
            pinMode(TrackSensorRightPin1, PinMode.Input);
            pinMode(TrackSensorRightPin2, PinMode.Input);

            //初始化超声波引脚模式
            pinMode(EchoPin, PinMode.Input);   //定义超声波输入脚

            

            digitalWrite(buzzer, HIGH);

            //舵机位置初始化
            servo_init();

        }


        public void Execute(string cmd) {

            switch (cmd) {
                case "run":
                    run(CurrnetSpeed, CurrnetSpeed);
                    break;
                case "break":
                    brake();
                    break;
                case "left":
                    left(CurrnetSpeed);
                    break;
                case "right":
                    right(CurrnetSpeed);
                    break;
                case "back":
                    back(CurrnetSpeed);
                    break;
                default:
                    break;

            }

        }

        private async Task servo_init() {
            for (var i = 0; i < 10; i++)
            {
                await servo_pulse(ServoLeftRightPin, 90);
                await servo_pulse(ServoUpDownPin, 90);
                await servo_pulse(FrontServoPin, 90);
            }
        }
        /// <summary>
        /// 定义一个脉冲函数，用来模拟方式产生PWM值
        /// 实际脉冲为20ms,该脉冲高电平部分在0.5-2.5ms
        /// 控制0-180度
        /// </summary>
        /// <param name="v_iServoPin">舵机控制引脚</param>
        /// <param name="myangle">舵机转动指定的角度</param>
        /// <returns></returns>
        private async Task servo_pulse(int v_iServoPin, int myangle)
        {
            int PulseWidth;                    //定义脉宽变量
            PulseWidth = (myangle * 11) + 500; //将角度转化为500-2480 的脉宽值
            digitalWrite(v_iServoPin, HIGH);      //将舵机接口电平置高
            await Task.Delay(PulseWidth); // delayMicroseconds(PulseWidth);     //延时脉宽值的微秒数
            digitalWrite(v_iServoPin, LOW);       //将舵机接口电平置低
            await Task.Delay(20000 - PulseWidth); //  delay(20 - PulseWidth / 1000);     //延时周期内剩余时间
            return;
        }

        private void pinMode(int pin, PinMode mode) {
            WiringPi.Core.PinMode(pin, mode);
        }
        private void softPwmCreate(int pin, int min, int max) {
            WiringPi.Core.SoftPwmCreate(pin, min, max);
        }

        private void softPwmWrite(int pin, int val)
        {
            WiringPi.Core.PWMWrite(pin, Math.Abs(val));
        }
        private void digitalWrite(int pin, uint val) {
            WiringPi.Core.DigitalWrite(pin, val == HIGH ? DigitalValue.High: DigitalValue.Low);
        }

        private DigitalValue digitalRead(int pin) {
            return WiringPi.Core.DigitalRead(pin);
        }

        private void runSide(bool isLeft,int speed) {

            var pin = isLeft ? Left_motor_go : Right_motor_go;
            if (speed == 0)
            {
                digitalWrite(pin, LOW);
                digitalWrite(pin + 1, LOW);
            }
            else if (speed > 0)
            {
                digitalWrite(pin, HIGH);
                digitalWrite(pin + 1, LOW);
                softPwmWrite(Left_motor_pwm, speed);
            }
            else
            {
                digitalWrite(pin, HIGH);
                digitalWrite(pin + 1, LOW);
                softPwmWrite(Left_motor_pwm, Math.Abs(speed));
            }
             // TODO: softPwmWrite
        }

        /// <summary>
        /// 前进
        /// </summary>
        /// <param name="LeftCarSpeedControl">左轮速度</param>
        /// <param name="RightCarSpeedControl">右轮速度</param>
        public void run(int LeftCarSpeedControl, int RightCarSpeedControl) {

            runSide(true, LeftCarSpeedControl);
            runSide(false, RightCarSpeedControl);
        }
        /// <summary>
        /// 刹车
        /// </summary>
        public void brake() {

            runSide(true, 0);
            runSide(false, 0);

        }

        /// <summary>
        /// 左转
        /// </summary>
        /// <param name="speed"></param>
        public void left( int speed) {
            //左电机停止
            runSide(true, 0);
            //右电机前进
            runSide(false, speed);

        }
        /// <summary>
        /// 左旋
        /// </summary>
        /// <param name="speed"></param>
        public void spin_left(int speed)
        {
            //左电机后退
            runSide(true, speed * -1);
            //右电机前进
            runSide(false, speed);
        }

        /// <summary>
        /// 右转
        /// </summary>
        /// <param name="speed"></param>
        public void right(int speed)
        {
            //左电机前进
            runSide(true, speed);
            //右电机停止
            runSide(false, 0);
        }


        /// <summary>
        /// 右旋
        /// </summary>
        /// <param name="Speed"></param>
        public void spin_right(int speed)
        {
            //左电机前进
            runSide(true, speed);
            //右电机后退
            runSide(false, speed * -1);
        }

        public void back(int speed)
        {
            var realSpeed = speed * -1;
            //左电机后退
            runSide(true, realSpeed);
            //右电机后退
            runSide(false, realSpeed);
        }

        public async Task whistle()
        {
            WiringPi.Core.DigitalWrite(buzzer, DigitalValue.Low);//发声音
            await Task.Delay(100);//延时100ms
            WiringPi.Core.DigitalWrite(buzzer, DigitalValue.High);//不发声音
            await Task.Delay(1);//延时1ms
            WiringPi.Core.DigitalWrite(buzzer, DigitalValue.Low);//发声音
            await Task.Delay(200);//延时200ms
            WiringPi.Core.DigitalWrite(buzzer, DigitalValue.High);//不发声音
            await Task.Delay(2);//延时2ms

        }

        /// <summary>
        /// 七彩灯亮指定的颜色
        /// </summary>
        /// <param name="v_iRed">指定的颜色值（0-255）</param>
        /// <param name="v_iGreen">指定的颜色值（0-255）</param>
        /// <param name="v_iBlue">指定的颜色值（0-255）</param>
        public void color_led_pwm(int v_iRed, int v_iGreen, int v_iBlue)
        {
            softPwmWrite(LED_R, v_iRed);
            softPwmWrite(LED_G, v_iGreen);
            softPwmWrite(LED_B, v_iBlue);

        }


        
    }
}
