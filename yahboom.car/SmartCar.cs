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


        const uint HIGH = 1;
        const uint LOW = 0;

        int CurrnetSpeed = 100;


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


        private void softPwmWrite(int pin, int val)
        {
            WiringPi.Core.PWMWrite(pin, Math.Abs(val));
        }
        private void digitalWrite(int pin, uint val) {
            WiringPi.Core.DigitalWrite(pin, val == HIGH ? DigitalValue.High: DigitalValue.Low);
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
