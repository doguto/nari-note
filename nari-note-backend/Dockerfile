ARG RUBY_VERSION=3.3.7
FROM docker.io/library/ruby:$RUBY_VERSION-slim AS base

WORKDIR /nari-note

# sudo-apt clean と rm -rf /var/lib/apt/lists でインストール時のキャッシュをクリアする
RUN apt-get update -qq && \
    apt-get install -y --no-install-recommends \
      curl \
      default-mysql-client \
      libjemalloc2 \
      libvips && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists

ENV RAILS_ENV="development" \
    BUNDLE_PATH="/usr/local/bundle"

# 最終的なイメージサイズを削減するためにマルチステージビルドを行う
FROM base AS build

# Install packages needed to build gems
RUN apt-get update -qq && \
    apt-get install -y --no-install-recommends \
      build-essential \
      default-libmysqlclient-dev \
      git \
      neovim \
      libyaml-dev \
      pkg-config && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists

# Install application gems
COPY Gemfile Gemfile.lock ./
RUN bundle install && \
    rm -rf ~/.bundle/ "${BUNDLE_PATH}"/ruby/*/cache "${BUNDLE_PATH}"/ruby/*/bundler/gems/*/.git && \
    bundle exec bootsnap precompile --gemfile

# Copy application code
COPY . .

# Precompile bootsnap code for faster boot times
RUN bundle exec bootsnap precompile app/ lib/

# Adjust binfiles to be executable on Linux
RUN chmod +x bin/* && \
    sed -i "s/\r$//g" bin/* && \
    sed -i 's/ruby\.exe$/ruby/' bin/*

# Final stage for app image
FROM base

# Copy built artifacts: gems, application
COPY --from=build "${BUNDLE_PATH}" "${BUNDLE_PATH}"
COPY --from=build /rails /rails

# Run and own only the runtime files as a non-root user for security
RUN groupadd --system --gid 1000 rails && \
    useradd rails --uid 1000 --gid 1000 --create-home --shell /bin/bash && \
    chown -R rails:rails db log storage tmp
USER 1000:1000

# Entrypoint prepares the database.
ENTRYPOINT ["/rails/bin/docker-entrypoint"]

# Start server via Thruster by default, this can be overwritten at runtime
EXPOSE 80
CMD ["./bin/thrust", "./bin/rails", "server"]
