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

        public void run(int LeftCarSpeedControl, int RightCarSpeedControl) {


            WiringPi.Core.DigitalWrite(Left_motor_go, DigitalValue.High); //左电机前进使能
            WiringPi.Core.DigitalWrite(Left_motor_back, DigitalValue.Low); //左电机后退禁止
            WiringPi.Core.PWMWrite(Left_motor_pwm, LeftCarSpeedControl); // TODO: softPwmWrite

            WiringPi.Core.DigitalWrite(Right_motor_go, DigitalValue.High); //右电机前进使能
            WiringPi.Core.DigitalWrite(Right_motor_back, DigitalValue.Low); //右电机后退禁止
            WiringPi.Core.PWMWrite(Right_motor_pwm, LeftCarSpeedControl);


        }
    }
}
