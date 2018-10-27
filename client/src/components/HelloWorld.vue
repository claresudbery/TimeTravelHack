<template>
  <div class="hello">
    <h1>{{ msg }}</h1>
    <button @click="travelClicked">Travel in Time!</button>
    <button @click="Puck.write('LED1.set();\n');">On!</button>
    <button @click="Puck.write('LED1.reset();\n');">Off!</button>
  </div>
</template>

<script>
  import axios from 'axios'
  
export default {
  name: 'HelloWorld',
  data () {
    return {
      msg: 'Time Travel',
      isTimeOver: false,
      requestEndpoint: 'localhost:5001/api/moretimerequest',
      timeAlertEndpoint: 'localhost:5001/timealert'
    }
  },
  methods: {
    travelClicked () {
      console.log("Works!")
      axios.post(this.requestEndpoint)
        .then(response => {
          console.log(response.status)
        })
        .catch(error => {
          console.log("Uh Oh! " + error.message)
        })
      setInterval(function () {
        this.pollTimeAlert();
      }.bind(this), 1000);
    },
    pollTimeAlert () {
      axios.get(this.timeAlertEndpoint)
        .then(response => {
          if(response.data === 1)
          {
            console.log("Time's Up!")
            this.isTimeOver = true;
          }
          else {
            console.log("Time goes by, so slowly")
            this.isTimeOver = false;
          }
        })
        .catch(error => {
          console.log("Error retrieving time update: " + error.message)
        })
    }

  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
