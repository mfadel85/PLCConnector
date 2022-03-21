<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);
// set up params
function do_curl_request($url, $params=array()) {
  $ch = curl_init();
  curl_setopt($ch,CURLOPT_URL, $url);
  curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
  curl_setopt($ch, CURLOPT_COOKIEJAR, '/tmp/apicookie.txt');
  curl_setopt($ch, CURLOPT_COOKIEFILE, '/tmp/apicookie.txt');
 
  $params_string = '';
  if (is_array($params) && count($params)) {
    foreach($params as $key=>$value) {
      $params_string .= $key.'='.$value.'&'; 
    }
    rtrim($params_string, '&');
 
    curl_setopt($ch,CURLOPT_POST, count($params));
    curl_setopt($ch,CURLOPT_POSTFIELDS, $params_string);
  }
 
  //execute post
  $result = curl_exec($ch);
 
  //close connection
  curl_close($ch);
 
  return $result;
}

$url = 'http://localhost/store/index.php?route=api/login';
 
$fields = array(
  'username' => 'Default',
  'key' => 'LA6g3ogGx7lgceCO2uiFZJ4QCwfe93SY54OYi2Pvjnrnxr55sFygOMT1sATi0b7y439oTRZPlM2s9ZY9Qt6tLOYqyDcoVXmhNAChHV2wL3ptKSlaWxMtO5XHhsokshxVyCGiKgMMU775z4IVy549FxY4rTRYb8UVlGNHJBcDIQgkRXdWziUpkzJP6ybm1gUPIIVn5ehCXxQTiRXvqXc6dd0zz4MddwWnQdRMMbdS5wF2IszhxPunqKAYx2If6YZA',
);
 
$json = do_curl_request($url, $fields);
$data = json_decode($json);
var_dump($data);

var_dump($json);

$url = "http://localhost/store/";
$curl = curl_init();
curl_setopt_array($curl, array(
    CURLOPT_URL =>  "http://localhost/store/index.php?route=api/product",
    CURLOPT_RETURNTRANSFER => true,
    CURLOPT_ENCODING => "",
    CURLOPT_MAXREDIRS => 10,
    CURLOPT_TIMEOUT => 30,
    CURLOPT_HTTP_VERSION => CURL_HTTP_VERSION_1_1,
    CURLOPT_CUSTOMREQUEST => "GET",
    CURLOPT_HTTPHEADER => array(
        "cache-control: no-cache",
    ),
));

$response = curl_exec($curl);

$err = curl_error($curl);

curl_close($curl);
if ($err) {
    echo "cURL Error #:" . $err;
} else {
    echo $response;
}
die();