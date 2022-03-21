<?php

$apiKey = "LA6g3ogGx7lgceCO2uiFZJ4QCwfe93SY54OYi2Pvjnrnxr55sFygOMT1sATi0b7y439oTRZPlM2s9ZY9Qt6tLOYqyDcoVXmhNAChHV2wL3ptKSlaWxMtO5XHhsokshxVyCGiKgMMU775z4IVy549FxY4rTRYb8UVlGNHJBcDIQgkRXdWziUpkzJP6ybm1gUPIIVn5ehCXxQTiRXvqXc6dd0zz4MddwWnQdRMMbdS5wF2IszhxPunqKAYx2If6YZA"; //Whatever you put in System -> Users -> API

$url = "http://192.168.1.51/store/index.php?route=api/login&api_token=0";

$curl = curl_init($url);

$post = array(
    'username' => 'Default',
    'key' => $apiKey,
);

curl_setopt_array($curl, array(
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_POSTFIELDS => $post,
));

$raw_response = curl_exec($curl);
//var_dump($raw_response);
$response = json_decode($raw_response);
curl_close($curl);
var_dump($response);

$api_token = $response->api_token;
$curl = curl_init();


//$url = "http://192.168.1.51/store/index.php?route=api/order/sendOrders&api_token=" . $api_token;

//$curl = curl_init($url);
//curl_setopt($curl, CURLOPT_RETURNTRANSFER, TRUE);
//error_log("Sending Started");
//$raw_response = curl_exec($curl);
//error_log("ends");


