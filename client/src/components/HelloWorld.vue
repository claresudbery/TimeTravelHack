<template>
  <div class="hello">
    <h1>{{ msg }}</h1>
    <button @click="TravelClicked">Travel in Time!</button>
  </div>
</template>

<script>
export default {
  name: 'HelloWorld',
  data () {
    return {
      msg: 'Time Travel'
    }
  },
  methods: {
    TravelClicked () {
      console.log("Works!")
    }

  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>

</style>
