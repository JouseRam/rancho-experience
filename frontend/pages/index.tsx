import type { NextPage } from 'next'
import Head from 'next/head'

const Home: NextPage = () => {
  return (
    <>
      <Head>
        <title>Rancho Experience – Vive la Naturaleza</title>
        <meta name="description" content="Descubre la magia del rancho. Alojamiento, actividades y naturaleza." />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
      </Head>

      <main>
        {/* Hero */}
        <section className="min-h-screen flex flex-col items-center justify-center bg-ranch-green text-white text-center px-4">
          <h1 className="text-5xl font-serif font-bold mb-4">Rancho Experience</h1>
          <p className="text-2xl text-ranch-gold mb-8">Vive la Naturaleza</p>
          <a
            href="#reservaciones"
            className="bg-ranch-gold text-ranch-dark font-semibold px-8 py-3 rounded-full hover:bg-yellow-500 transition"
          >
            Reserva Ahora
          </a>
        </section>

        {/* Servicios */}
        <section id="servicios" className="py-20 px-4 max-w-6xl mx-auto text-center">
          <h2 className="text-3xl font-serif text-ranch-brown mb-12">Nuestros Servicios</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {['Cabalgata', 'Glamping', 'Pesca'].map((s) => (
              <div key={s} className="bg-white rounded-2xl shadow p-8">
                <h3 className="text-xl font-semibold text-ranch-green mb-2">{s}</h3>
                <p className="text-gray-500">Descripción del servicio próximamente.</p>
              </div>
            ))}
          </div>
        </section>

        {/* Contacto */}
        <section id="reservaciones" className="py-20 bg-ranch-brown text-white text-center px-4">
          <h2 className="text-3xl font-serif mb-4">Contáctanos</h2>
          <p className="mb-8">info@ranchoexperience.com</p>
        </section>
      </main>
    </>
  )
}

export default Home
